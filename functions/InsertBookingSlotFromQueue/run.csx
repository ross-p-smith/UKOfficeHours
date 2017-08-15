#load "..\Shared\httpUtils.csx"
#load "..\Shared\CompleteAppointmentDTO.csx"
#load "..\Shared\TechnicalResource.csx"
#load "..\Shared\EncryptionUtils.csx"
#load "..\Shared\isv.csx"

#r "Microsoft.ServiceBus"
#r "Microsoft.WindowsAzure.Storage"

using System;    
using System.Threading; 
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure; 
using Microsoft.Azure.KeyVault; 
using Microsoft.Azure.KeyVault.Core; 
using System.Text;
using System.Web;
using System.Web.Http;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json; 
using Microsoft.WindowsAzure.Storage.Table.Queryable;
 
public static void Run(string inQueueMessage, CloudTable outBookingTable, IAsyncCollector<BookingSlot> outQueueNotification, CloudTable isvTable, CloudTable technicalResourceTable, IAsyncCollector<CompleteAppointmentDTO> outQueueUpdate, TraceWriter log)
{
    BookingSlot calendarEvent = JsonConvert.DeserializeObject<BookingSlot>(inQueueMessage);

    if (string.IsNullOrEmpty(calendarEvent.MailID))
    {
        throw new ArgumentException("Mail Id is not present");
    }

    // Find the booking slot based on an existing Calendar Id. 
    IQueryable<BookingSlot> bookingSlots = (from booking in outBookingTable.CreateQuery<BookingSlot>() select booking);
    BookingSlot existingBookingSlot = bookingSlots.Where<BookingSlot>(slot => slot.MailID == calendarEvent.MailID).SingleOrDefault();
    log.Info("Bookings " + existingBookingSlot);

    if (existingBookingSlot == null)
    {
        // A new placeholder has been created in Outlook, create a matching entry in Office Hours and send a message
        // back over Service Bus with the Partition Key and Row Key in case it ever gets updated (Use Case above)
        log.Info("New booking coming from queue on " + calendarEvent.StartDateTime.ToString() + " for " + calendarEvent.Duration + " minutes.");

        calendarEvent.PartitionKey = calendarEvent.StartDateTime.Date.ToString("yyyyMM");
        calendarEvent.RowKey = Guid.NewGuid().ToString(); 

        TableOperation add = TableOperation.Insert(calendarEvent);
        outBookingTable.Execute(add);

        log.Info("Update queue with Office Hours Identifier");
        outQueueNotification.AddAsync(calendarEvent);
    }
    else
    {
        log.Info("Existing booking found. Booking Code: " + existingBookingSlot.BookingCode);

        if (existingBookingSlot.BookingCode == "None")
        {
            // It is safe to change this booking slot as there has been no booking made
            if (!existingBookingSlot.IsSameTimeSlot(calendarEvent))
            {
                // The time slot has changed
                log.Info("Updating placeholder to " + calendarEvent.StartDateTime.ToString());

                existingBookingSlot.StartDateTime = calendarEvent.StartDateTime;
                existingBookingSlot.EndDateTime = calendarEvent.EndDateTime;
                existingBookingSlot.Duration = calendarEvent.Duration;

                TableOperation update = TableOperation.Replace(existingBookingSlot);
                outBookingTable.Execute(update);
            }
            else
            {
                log.Info("Attempt to update a slot with the same details - do nothing");
            }
        }
        else
        {            
            // A meeting has changed in Outlook. However, this meeting has been booked since
            // this message arrived
            log.Info($"Attempted to update a meeting which has since been booked with code '{existingBookingSlot.BookingCode}'.");

            // Send all the details back to create a new meeting.
            isv queryisv = (from isv in isvTable.CreateQuery<isv>() select isv)
                .Where(e => e.CurrentCode == existingBookingSlot.BookingCode)
                .WithOptions(GetTableRequestOptionsWithEncryptionPolicy()).FirstOrDefault();

            if (queryisv != null)
            {
                log.Info($"Located ISV:{queryisv.Name} Code: {queryisv.CurrentCode}");  
            }
            else
            {
                log.Info("Unable to find ISV.");
            }

            // Find the TE master data record 
            TechnicalResource chkte = (from te in technicalResourceTable.CreateQuery<TechnicalResource>() select te)
                .Where(e => e.RowKey == existingBookingSlot.TechnicalEvangelist && e.PartitionKey == "ALL")
                .FirstOrDefault();

            if (chkte != null)
            {
                CompleteAppointmentDTO appointment = new CompleteAppointmentDTO()
                {
                        MailID = existingBookingSlot.MailID,
                        StartDate = existingBookingSlot.StartDateTime,
                        EndDate = existingBookingSlot.EndDateTime,
                        Duration = existingBookingSlot.Duration,
                        TEMail = existingBookingSlot.TechnicalEvangelist,
                        TEName = chkte.TEName,
                        TESkypeData = chkte.SkypeLink, 
                        PBEMail = existingBookingSlot.PBE,
                        ISVMail = queryisv.ContactEmail,
                        ISVName = queryisv.Name,
                        ISVContact = queryisv.ContactName                 
                };

                outQueueUpdate.AddAsync(appointment);
            }
        }
    }
}

public class BookingSlot : TableEntity
{
    public BookingSlot()
    {
        CreatedDateTime = DateTime.Now;
        BookedToISV = "None";
        BookingCode = "None";
        PBE = "None";
    }
    
    public string TechnicalEvangelist {get; set;}
    public DateTime StartDateTime {get; set;}
    public DateTime EndDateTime {get; set;}
    public DateTime CreatedDateTime { get; set; }

    public string MailID {get;set;}

    public int Duration
    {
        get
        {
            TimeSpan ts = EndDateTime - StartDateTime;
            return ts.Minutes + (ts.Hours * 60);
        }
        set { }
    }

    public string BookedToISV { get; set; }
    public string BookingCode { get; set; }
    public string PBE { get; set; }

    public bool IsSameTimeSlot(BookingSlot slot)
    {
        if (this.StartDateTime == slot.StartDateTime && this.EndDateTime == slot.EndDateTime)
        {
            return true;
        }

        return false;
    }
}