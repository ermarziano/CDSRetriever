using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Resources;
using CDSRetriever.Properties;
using System.Threading;
using JsonCreator;
using System.Linq;

namespace CDSRetriever
{
    public class Program
    {
        static string CDSpath = "";

        public class Item
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public DateTime CreationDate { get; set; }
        }
        public static void Main(string[] args)
        {
            Console.Write("What do you need? Direct download (1) or search (2)? ");
            string choice = Console.ReadLine();
            if (choice == "1")
            {
                Console.Write("Insert the CDS Id: ");
                string id = Console.ReadLine();
                var client = new RestClient("http://ide.ent.bhicorp.com/CDS/api/");
                client.UseNewtonsoftJson();
                client.Authenticator = new HttpBasicAuthenticator("CDS", "9&P$7Qu7=&DnLUJh");
                var request = new RestRequest("/CDS/" + id);
                var response = client.Get(request);
                List<Item> item = JsonConvert.DeserializeObject<List<Item>>(response.Content);
                DownloadCDS(item[0]);
            }
            else
            {
                while (true)
                {
                    List<Item> OggettiTrovati = new();
                    Console.Write("Insert the number of days you want to search for: ");
                    int days = int.Parse(Console.ReadLine());
                    var client = new RestClient("http://ide.ent.bhicorp.com/CDS/api/");
                    client.UseNewtonsoftJson();
                    client.Authenticator = new HttpBasicAuthenticator("CDS", "9&P$7Qu7=&DnLUJh");
                    var dateToSearch = DateTime.Today.AddDays(-days);
                    var request = new RestRequest("CDSes/" + dateToSearch.ToString("MM-dd-yy"), DataFormat.Json);
                    var response = client.Get(request);
                    Console.WriteLine("Searching in CDS Assistant...");
                    OggettiTrovati = JsonConvert.DeserializeObject<List<Item>>(response.Content);
                    int totalFound = OggettiTrovati.Count;
                    OggettiTrovati.RemoveAll(IsAlreadyDownloaded);
                    Console.WriteLine(string.Format("Found {0} items ({1} already downloaded, hidden)", totalFound, totalFound - OggettiTrovati.Count));
                    while (OggettiTrovati.Count > 0)
                    {
                        int count = 1;
                        foreach (Item i in OggettiTrovati)
                        {
                            Console.WriteLine($"Item {count}: Name - {i.Name}, Id - {i.Id}");
                            count++;
                        }
                        Console.Write($"List has {totalFound} items ({totalFound - OggettiTrovati.Count} already downloaded). Which one do you want? (type 1,2,3 etc): ");
                        int itemId = int.Parse(Console.ReadLine());
                        Item item = OggettiTrovati[itemId - 1];
                        DownloadCDS(item);
                        OggettiTrovati.Remove(item);
                        if (OggettiTrovati.Count == 0)
                        {
                            Console.WriteLine("No more items in the list.");
                            break;
                        }
                        Console.WriteLine("Cds Downloaded. Request another one inside the list?: (y)es or (n)o");
                        if (Console.ReadLine().ToLower().Equals("n"))
                        {
                            break;
                        }
                    }
                    Console.Write("Cds Downloaded. Do Another search?: (y)es or (n)o");
                    String cont = Console.ReadLine();
                    if (cont.ToLower().Equals("n"))
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// A CDS counts as downloaded only if list.json exists, since it is written as the last step of DownloadCDS.
        /// </summary>
        private static bool IsAlreadyDownloaded(Item item)
        {
            return File.Exists(Resources.DownloadFolder + item.Name + @"\list.json");
        }

        private static bool FindFilesFromCDS(Item item, out string status)
        {
            string itemInfo = item.Name.Replace("_CDS", "");
            string directory = Resources.DownloadFolder + item.Name + @"\";
            bool foundDS = Directory.EnumerateFiles(directory, itemInfo + "_DS_*").Any();


            File.Exists(Resources.DownloadFolder + "_DS_");
            bool foundSDS = File.Exists(Resources.DownloadFolder + "_SDS_");
            if (!foundDS && !foundSDS)
            {
                status = "No files found for " + itemInfo;
                return false;
            }
            if (foundDS && foundSDS)
            {
                File.Move(Re)
                status = $"Both DS and SDS found for {itemInfo}, copied them to folder {directory}";
                return true
            }


        public static void DownloadCDS(Item item)
        {
            Dictionary<string, string> Files = new Dictionary<string, string>();
            var client = new RestClient("http://ide.ent.bhicorp.com/CDS/api/");
            client.UseNewtonsoftJson();
            client.Authenticator = new HttpBasicAuthenticator("CDS", "9&P$7Qu7=&DnLUJh");
            var request = new RestRequest("/CDS/" + item.Id);
            var response = client.Get(request);
            string directory = Resources.DownloadFolder + item.Name + @"\";
            string directoryList = @"C:\Temp\" + item.Name + @"\";
            Directory.CreateDirectory(directory);
            File.WriteAllText(directory + item.Name + ".json", response.Content);
            CDSpath = directory + item.Name + ".json";
            request = new RestRequest("CDSPictures/" + item.Id);
            response = client.Get(request);
            Console.WriteLine("Downloading CDS Pictures");
            try
            {
                List<ImageItem> Immagini = JsonConvert.DeserializeObject<List<ImageItem>>(response.Content);
                if (Immagini.Count > 0)
                {
                    SaveImage(Immagini[0].Image, 800, 600, directory + item.Name.ToLower() + "_td.png");
                    SaveImage(Immagini[0].Image, 150, 150, directory + item.Name.ToLower() + "_preview.png");
                    Files.Add("front", directoryList + item.Name.ToLower() + "_td.png");
                    Files.Add("preview", directoryList + item.Name.ToLower() + "_preview.png");
                }
                if (Immagini.Count > 1)
                {
                    SaveImage(Immagini[1].Image, 800, 600, directory + item.Name.ToLower() + "_tds.png");
                    Files.Add("side", directoryList + item.Name.ToLower() + "_tds.png");
                }
                if (Immagini.Count > 2)
                {
                    SaveImage(Immagini[2].Image, 800, 600, directory + item.Name.ToLower() + "_tdt.png");
                    Files.Add("top", directoryList + item.Name.ToLower() + "_tdt.png");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(response.StatusCode);
                Console.WriteLine("Old picture format, only one item.");
                var result = response.Content;
                Console.WriteLine(result);
                ImageItem Immagine = JsonConvert.DeserializeObject<ImageItem>(result);
                Console.WriteLine(Immagine.Image.ToString());
                SaveImage(Immagine.Image, 800, 600, directory + item.Name.ToLower() + "_td.png");
                SaveImage(Immagine.Image, 150, 150, directory + item.Name.ToLower() + "_preview.png");
                Files.Add("front", directoryList + item.Name.ToLower() + "_td.png");
                Files.Add("preview", directoryList + item.Name.ToLower() + "_preview.png");
            }
            ResourceManager res = new ResourceManager(typeof(Resources));
            var proc = new Process();
            Console.WriteLine("Download CDS Documents");
            //create custom bat file to run in the other server
            CreateBat(item, directory);
            Console.WriteLine("Put manually the files in directory " + directory);
            Console.WriteLine("Type DONE when you're done");
            string cont = Console.ReadLine();
            RenameFiles(Files, directoryList, directory);
            File.WriteAllText(directory + "list.json", JsonConvert.SerializeObject(Files));
        }

        private static void CreateBat(Item item, string directory)
        {
            string batname = item.Name + ".bat";
            using StreamWriter sw = File.CreateText(directory + batname);
            sw.WriteLine(@"@echo off");
            sw.WriteLine(@"md " + directory);
            sw.WriteLine(@"START CdsExportGenerator\CdsExportGenerator.EXE xjB2gYn2ZB=gW9Ws " + item.Id + " Scouting " + directory + @" FRKVBHNPAVAD01.ent.bhicorp.com\IDE CDSAssistant");
            sw.WriteLine(@"START CdsExportGenerator\CdsExportGenerator.EXE xjB2gYn2ZB=gW9Ws " + item.Id + " DataSheet " + directory + @" FRKVBHNPAVAD01.ent.bhicorp.com\IDE CDSAssistant");
        }

        public string getPath()
        {
            return CDSpath;
        }
        public static void RenameFiles(IDictionary<string, string> list, string path, string pathInput)
        {
            string[] files = Directory.GetFiles(pathInput);
            Console.WriteLine(path);
            foreach (string file in files)
            {
                if (file.ToLower().EndsWith(".pdf"))
                {
                    string onlyFileName = file.Substring(file.LastIndexOf('\\') + 1);
                    Console.WriteLine(onlyFileName);
                    string[] pieces = onlyFileName.ToLower().Split('_');
                    string nameForList = path + pieces[0] + "_cds_" + pieces[1] + "_sds.pdf";
                    string nameForFile = pathInput + pieces[0] + "_cds_" + pieces[1] + "_sds.pdf";
                    list.Add("sds", nameForList);
                    File.Move(file, nameForFile);
                }
                if (file.ToLower().EndsWith(".docx"))
                {
                    string onlyFileName = file.Substring(file.LastIndexOf('\\') + 1);
                    Console.WriteLine(onlyFileName);
                    string[] pieces = onlyFileName.ToLower().Split('_');
                    string nameForList = path + pieces[0] + "_cds_" + pieces[1] + "_ds.docx";
                    string nameForFile = pathInput + pieces[0] + "_cds_" + pieces[1] + "_ds.docx";
                    list.Add("ds", nameForList);
                    File.Move(file, nameForFile);
                }
                Console.WriteLine(file);
            }
        }

        public static void SaveImage(string source, int maxWidth, int maxHeight, string path)
        {
            Image sourceImage = (Bitmap)(new ImageConverter()).ConvertFrom(Convert.FromBase64String(source));
            // Determine which ratio is greater, the width or height, and use
            // this to calculate the new width and height. Effectually constrains
            // the proportions of the resized image to the proportions of the original.
            double xRatio = (double)sourceImage.Width / maxWidth;
            double yRatio = (double)sourceImage.Height / maxHeight;
            double ratioToResizeImage = Math.Max(xRatio, yRatio);
            int newWidth = (int)Math.Floor(sourceImage.Width / ratioToResizeImage);
            int newHeight = (int)Math.Floor(sourceImage.Height / ratioToResizeImage);

            // Create new image canvas -- use maxWidth and maxHeight in this function call if you wish
            // to set the exact dimensions of the output image.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            Rectangle destinationRect = new Rectangle(0, 0, newWidth, newHeight);

            // Render the new image, using a graphic object
            using (Graphics newGraphic = Graphics.FromImage(newImage))
            {
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    newGraphic.DrawImage(sourceImage, destinationRect, 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, wrapMode);
                }

                // Set the background color to be transparent (can change this to any color)
                newGraphic.Clear(Color.Transparent);

                // Set the method of scaling to use -- HighQualityBicubic is said to have the best quality
                newGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Apply the transformation onto the new graphic
                Rectangle sourceDimensions = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                Rectangle destinationDimensions = new Rectangle(0, 0, newWidth, newHeight);
                newGraphic.DrawImage(sourceImage, destinationDimensions, sourceDimensions, GraphicsUnit.Pixel);
            }
            // Image has been modified by all the references to it's related graphic above. Return changes.
            newImage.Save(path, ImageFormat.Png);
        }
    }
}