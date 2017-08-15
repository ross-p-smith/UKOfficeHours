using System;    
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.Azure; 
using Microsoft.Azure.KeyVault; 
using Microsoft.Azure.KeyVault.Core; 
using System.Net;
using System.IO;
using Microsoft.WindowsAzure.Storage.Table.Queryable;

public class TechnicalResource : TableEntity
{
    // RowKey is the User's Alias
    // PartitionKey is a static 'ALL' value
    [EncryptProperty]
    public string SkypeLink {get; set;}
    public string TEName {get; set;}
}