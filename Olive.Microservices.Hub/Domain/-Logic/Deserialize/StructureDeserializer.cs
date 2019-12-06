using Olive;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain
{
    public class StructureDeserializer
    {
        const string MENU_FILE_NAME = "hub-menu";
        const string MENU_FOLDER_NAME = "hub";
        static HashSet<XElement> elements = new HashSet<XElement>();
        static readonly IBlobStorageProvider blobStorageProvider = Context.Current.GetService<IBlobStorageProvider>();

        public async static Task Load()
        {
            if (Service.All == null)
                await LoadServices();

            if (Feature.All == null)
                await LoadFeatures();

            if (Board.All == null)
                await LoadBoards();
        }

        public static Task ReLoad()
        {
            Service.All = null;
            Feature.All = null;
            Board.All = null;
            return Load();
        }

        static async Task LoadServices()
        {
            if (Service.All == null)
            {
                var environment = Context.Current.Environment().EnvironmentName.ToLower();

                Service.All = await ReadXml("Services.xml").Select(x => new Service
                {
                    Name = x.GetCleanName(),
                    UseIframe = x.GetValue<bool?>("@iframe") ?? false,
                    BaseUrl = x.GetValue<string>("@" + environment) ?? x.GetValue<string>("@url"),
                    Icon = x.GetValue<string>("@icon"),
                    InjectSingleSignon = x.GetValue<bool?>("@sso") ?? false

                }).ToList();
            }
        }

        static async Task LoadFeatures(bool forceToLoad = false)
        {
            if (Feature.All == null || forceToLoad)
            {
                Feature.All = (await GetFeatureDefinitions(forceToLoad)
                    .SelectMany(x => x.GetAllFeatures())
                    .ExceptNull().ToList())
                    .OrderBy(x => x.PositionOrder);

                foreach (var item in Feature.All)
                    item.Children = Feature.All.Where(x => x.Parent == item);
            }
        }

        static async Task LoadBoards()
        {
            if (Board.All == null)
            {
                Board.All = await ReadXml("Boards.xml").Select(x => new Board(x)).ToList();
            }
        }

        static async Task<IEnumerable<XElement>> ReadXml(string file)
        {
            var fileContent = await AppDomain.CurrentDomain.WebsiteRoot().GetFile(file)
                .ReadAllTextAsync();

            return fileContent.To<XDocument>().Root.Elements();
        }

        static async Task<FeatureDefinition[]> GetFeatureDefinitions(bool updateS3)
        {
            var root = new FeatureDefinition(null, new XElement("ROOT"));

            try
            {
                if (elements.Any())
                {
                    return SetMenu(root, updateS3);
                }

                // Get menu from S3
                var menuFile = new Blob(MENU_FILE_NAME) { FolderName = MENU_FOLDER_NAME };

                // In the menu develop mode we need to remove menu file from S3
                if (Config.Get("Menu:Develop").To<bool>())
                    await blobStorageProvider.DeleteAsync(menuFile);

                var s3File = await blobStorageProvider.FileExistsAsync(menuFile);

                if (s3File)
                {
                    var menuFileContent = await blobStorageProvider.LoadAsync(menuFile);

                    var menuFileText = System.Text.Encoding.ASCII.GetString(menuFileContent);

                    var xElements = Newtonsoft.Json.JsonConvert.DeserializeObject<HashSet<XElement>>(menuFileText);

                    elements.AddRange(xElements);

                    return SetMenu(root, updateFile: false);
                }

                var files = AppDomain.CurrentDomain.WebsiteRoot().GetFiles("Features*.xml");

                foreach (var item in files)
                {
                    var xElements = await ReadXml(item.Name);

                    elements.AddRange(xElements);
                }

                return SetMenu(root);
            }
            catch (Exception ex)
            {
                Log.For<StructureDeserializer>().Error(ex);

                return Enumerable.Empty<FeatureDefinition>().ToArray();
            }
        }

        static FeatureDefinition[] SetMenu(FeatureDefinition root, bool updateFile = true)
        {
            // Save menu at the S3
            if (updateFile)
            {
                var jsonFile = Newtonsoft.Json.JsonConvert.SerializeObject(elements, Newtonsoft.Json.Formatting.Indented);

                var s3File = new Blob(jsonFile.ToBytes(System.Text.Encoding.ASCII), MENU_FILE_NAME) { FolderName = MENU_FOLDER_NAME };

                blobStorageProvider.SaveAsync(s3File);
            }

            return elements.Select(x => new FeatureDefinition(root, x)).ToArray();
        }

        public async static Task WatchForUpdatedMenus()
        {
            if (Service.All == null) return;

            foreach (var service in Service.All.Where(x => x.UseIframe == false))
            {
                try
                {
                    var url = service.BaseUrl;

                    // Get encrypted menu from micro-services.
                    var menuResult = await new ApiClient($"{url}/api/menu")
                        .Cache(CachePolicy.FreshOrNull)
                        .OnFallBack(ApiFallBackEventPolicy.Silent)
                        .Get<string>();

                    if (menuResult.IsEmpty()) continue;

                    var decrypted = Olive.Security.Encryption.Decrypt(menuResult.Trim('"'), Config.Get("DataEncryption:Menu", "Hub.Menu"));

                    var items = decrypted.To<XDocument>().Root.Elements();

                    foreach (var item in items)
                    {
                        if (elements.None(x => x.Name == item.Name))
                            elements.Add(item);
                        else
                        {
                            var selectedElement = elements.First(x => x.Name == item.Name);

                            foreach (var element in item.Elements())
                            {
                                if (selectedElement.Descendants().None(x => x.Name == element.Name))
                                {
                                    selectedElement.Add(element);
                                }
                            }
                        }
                    }

                    Log.For<StructureDeserializer>().Info($"Menu received from {service.Name}");
                    await LoadFeatures(true);
                }
                catch (Exception ex)
                {
                    Log.For<StructureDeserializer>().Error(ex);
                }
            }

            Log.For<StructureDeserializer>().Info("Menu has been updated.");
        }
    }
}