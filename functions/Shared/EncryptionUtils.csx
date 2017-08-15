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

public static TableRequestOptions GetTableRequestOptionsWithEncryptionPolicy()
{
    KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(async (string authority, string resource, string scope) => {
            ClientCredential credential = new ClientCredential(Environment.GetEnvironmentVariable("KVClientId"), Environment.GetEnvironmentVariable("KVKey"));
            AuthenticationContext ctx = new AuthenticationContext(new Uri(authority).AbsoluteUri, false);
            AuthenticationResult result = await ctx.AcquireTokenAsync(resource, credential);
            return result.AccessToken;}
    );
    IKey cloudKey1 = cloudResolver.ResolveKeyAsync(Environment.GetEnvironmentVariable("KVKeyID"), CancellationToken.None).GetAwaiter().GetResult(); 
    TableEncryptionPolicy encryptionPolicy = new TableEncryptionPolicy(cloudKey1, cloudResolver);
    TableRequestOptions rqOptions = new TableRequestOptions() { EncryptionPolicy = encryptionPolicy };
    return rqOptions; 
} 