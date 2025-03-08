using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using MVC.Models;

namespace MVC.Business
{
    public class BlobController
    {
        private ApplicationConfiguration _applicationConfiguration { get; }

        public BlobController(IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _applicationConfiguration = options.Value;
            Console.WriteLine($"BlobController initialized with connection string: {_applicationConfiguration.BlobConnectionString}");
            Console.WriteLine($"Unvalidated container: {_applicationConfiguration.UnvalidatedBlob}");

            // Ensure containers exist on startup
            EnsureContainersExistAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureContainersExistAsync()
        {
            try
            {
                Console.WriteLine("Checking if containers exist...");
                BlobServiceClient serviceClient = new BlobServiceClient(_applicationConfiguration.BlobConnectionString);

                // Create unvalidated container if it doesn't exist
                BlobContainerClient unvalidatedContainer = serviceClient.GetBlobContainerClient(_applicationConfiguration.UnvalidatedBlob);
                await unvalidatedContainer.CreateIfNotExistsAsync();
                Console.WriteLine($"Container '{_applicationConfiguration.UnvalidatedBlob}' created or exists");

                // Create validated container if it doesn't exist
                BlobContainerClient validatedContainer = serviceClient.GetBlobContainerClient(_applicationConfiguration.ValidatedBlob);
                await validatedContainer.CreateIfNotExistsAsync();
                Console.WriteLine($"Container '{_applicationConfiguration.ValidatedBlob}' created or exists");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring containers exist: {ex.Message}");
                // Don't throw here to allow the application to start even if containers can't be created
            }
        }

        public async Task<string> PushImageToBlob(IFormFile formFile, Guid imageGuid)
        {
            Console.WriteLine($"PushImageToBlob called for file {formFile.FileName} with guid {imageGuid}");

            // Conversion du fichier recu en IFormFile a Byte[]. 
            // Ensuite le Byte[] sera envoyer au BlobStorage en utilisant un Guid comme identifiant.
            // Nous allons garder le Guid et créer un URL.
            using (MemoryStream ms = new MemoryStream())
            {
                if (ms.Length < 40971520)
                {
                    await formFile.CopyToAsync(ms);
                    Console.WriteLine($"File copied to memory stream, length: {ms.Length}");

                    //Création du service connection au Blob
                    BlobServiceClient serviceClient = new BlobServiceClient(_applicationConfiguration.BlobConnectionString);

                    //Création du client pour le Blob
                    BlobContainerClient blobClient = serviceClient.GetBlobContainerClient(_applicationConfiguration.UnvalidatedBlob);

                    // Make sure the container exists
                    await blobClient.CreateIfNotExistsAsync();
                    Console.WriteLine($"Ensuring container '{_applicationConfiguration.UnvalidatedBlob}' exists");

                    //Reinitialize le Stream
                    ms.Position = 0;

                    //Envoie de l'image sur le blob
                    Console.WriteLine($"Uploading blob with ID: {imageGuid}");
                    await blobClient.UploadBlobAsync(imageGuid.ToString(), ms);
                    Console.WriteLine("Upload completed successfully");

                    string url = blobClient.Uri.AbsoluteUri + "/" + imageGuid.ToString();
                    Console.WriteLine($"Generated URL: {url}");
                    return url;
                }
                else
                {
                    Console.WriteLine("File too large");
                    throw new ExceptionFilesize();
                }
            }
        }
    }

    // Exception créer par la BusinessLayer pour expliquer que le fichier est trop gros.
    public class ExceptionFilesize : Exception
    {
        public ExceptionFilesize() : base("File size exceeds the maximum allowed size of 40MB") { }
    }
}