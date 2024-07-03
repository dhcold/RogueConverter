using static System.Net.Mime.MediaTypeNames;

namespace RogueMaker
{
    static class Program
    {
        [STAThread]
        
        static void Main()
        {
            // CURRENT DIRECTORY
            string directoryPath = Environment.CurrentDirectory;

            // SCANNING CURRENT DIRECTORY FOR .rbe FILES
            string[] rbeFiles = Directory.GetFiles(directoryPath, "*.rbe");

            // FOUND .rbe FILES OUTPUT
            Console.WriteLine("\nFound .rbe files in current directory:");
            for (int i = 0; i < rbeFiles.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(rbeFiles[i])}");
            }

            // USER SELECTING MAP VIA CONSOLE INPUT
            Console.WriteLine("\nEnter the number of the map to convert:\n");
            string input = Console.ReadLine();

            // CHECKING USER'S INPUT
            if (int.TryParse(input, out int mapIndex) && mapIndex > 0 && mapIndex <= rbeFiles.Length)
            {
                string selectedFilePath = rbeFiles[mapIndex - 1];
                // PLACE YOUR MAP INTO PROJECT FOLDER
                MapHandler mapHandler = new MapHandler();
                string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);

                // LOADING THE MAP
                Console.WriteLine("\n**********************" +
                    "\n* Loading selected map...");                
                MapObject map = mapHandler.LoadMap(selectedFilePath);
                FileInfo fileInfo = new FileInfo(selectedFilePath);
                string mapFileName = Path.GetFileName(selectedFilePath);
                // GET FILE BYTES
                long fileSizeInBytes = fileInfo.Length;

                // CONVERTS BYTES INTO MB
                double fileSizeInMB = (double)fileSizeInBytes / (1024 * 1024);

                
                
                Console.WriteLine("**********************" + 
                    $"\n* Map: {mapFileName}" + 
                    $"\n* Version: {map.ver}" + 
                    $"\n* Author: {map.authorName}" +
                    "\n**********************" + 
                    $"\n* Blocks: {map.blocks.Count()}" + 
                    $"\n* Entities: {map.entities.Count()}" + 
                    $"\n* Map file size: {fileSizeInMB:F2} MB" +
                    "\n**********************" +
                    "\n* Create different props for different block materials?" +
                    "\n* Enter [1] to create different props with different materials \n* Enter [0] to skip (every prop material will be the same) \n* Enter [anything else] to select another map:\n");
                string materialInput = Console.ReadLine().Trim();
                if (materialInput != "1" && materialInput != "0")
                {
                    Main();
                    return;
                }
                else
                {
                    Console.WriteLine("\n**********************" +
                        "\n* (This can take UP TO 15(?) MINUTES if there is a lot of blocks)" +
                        "\n* Starting conversion...");

                    // TRANSFORMING BLOCKS INTO PROPS
                    MapEditor mapEditor = new MapEditor();
                    bool differMaterials = materialInput == "1" ? true : false;
                    map = mapEditor.PortMap(map, differMaterials);
                    Console.WriteLine($"* Entities (new count): {map.entities.Count()}");


                    // SAVING THE MAP
                    
                    string targetDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DiaboticalRogue\Maps\";
                    if (!Directory.Exists(targetDir))
                    {
                        targetDir = Environment.CurrentDirectory + @"\";
                    }
                    fileName = $"{fileName}_rogue";
                    string savePath = Path.Combine(targetDir, $"{fileName}.rbe");
                    mapHandler.SaveMap(map, savePath);

                    FileInfo newFileInfo = new FileInfo(savePath);
                    fileSizeInBytes = newFileInfo.Length;
                    fileSizeInMB = (double)fileSizeInBytes / (1024 * 1024);                    
                    Console.WriteLine(
                        $"* NEW file size: {fileSizeInMB:F2} MB" +
                        $"\n******* SUCCESS! ******" +
                        $"\n* Saving converted map into DiaboticalRogue appdata folder (into game-folder)..." + 
                        $"\n* To edit your map type in console: " +
                        $"\n* /edit {fileName}" + 
                        "\n**********************");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number corresponding to the map.");
            }

            Console.WriteLine("\nPress any key to choose another map or press 'ESC' to exit...");
            var key = Console.ReadKey();
            if (key.Key != ConsoleKey.Escape)
            {
                Main(); return;
            };
        }
    }
}