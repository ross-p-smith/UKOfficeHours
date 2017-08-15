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

public class CompleteAppointmentDTO
{
    public string MailID {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}    
    public int Duration {get;set;}     
    public string TEMail {get;set;}
    public string TEName {get;set;}
    public string TESkypeData {get;set;}    
    public string PBEMail {get;set;}
    public string ISVMail {get;set;}
    public string ISVName {get;set;}
    public string ISVContact {get;set;}        
}