using System.Globalization;
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
                MapEditor mapEditor = new MapEditor();
                string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);

                // LOADING THE MAP
                Console.WriteLine(
                    "\n" + defaultLane() +
                    "\n* Loading selected map...");                
                MapObject map = mapHandler.LoadMap(selectedFilePath);
                FileInfo fileInfo = new FileInfo(selectedFilePath);
                string mapFileName = Path.GetFileName(selectedFilePath);
                // GET FILE BYTES
                long fileSizeInBytes = fileInfo.Length;

                // CONVERTS BYTES INTO MB
                double fileSizeInMB = (double)fileSizeInBytes / (1024 * 1024); 

                // MAP INFO
                Console.WriteLine
                    (defaultLane() + 
                    $"\n* Map: {mapFileName}" + 
                    $"\n* Version: {map.ver}" + 
                    $"\n* Author: {map.authorName}" +
                    "\n" + defaultLane() +
                    $"\n* Blocks: {map.blocks.Count()}" + 
                    $"\n* Entities: {map.entities.Count()}" + 
                    $"\n* Map file size: {fileSizeInMB:F2} MB");

                /// SELECT OPERATION
                /// 1 - Convert blocks
                /// 2 - Rescale map
                /// 0 (or anything else) Back
                Console.WriteLine(
                    "\n" + defaultLane() +
                    "\n* What to do with the map?" +
                    "\n* [1] * Convert blocks (replaces every block with prop)" + 
                    "\n* [2] * Rescale map" +
                    "\n*  *  *" +
                    "\n* [0] * Back\n");
                string operationInput = Console.ReadLine();

                // 1. CONVERTING BLOCKS
                if (operationInput == "1" || operationInput == "2")
                {
                    if (operationInput == "1")
                    {
                        fileName = $"{fileName}_rogue";
                        /// SELECT CONVERSION METHOD
                        /// 1 - function checks each block material, and will create entity once new material found on next block
                        /// 2 - function doesnt check materials. All converted props will have same material
                        /// 0 (or anything else) Back

                        Console.WriteLine("\n" + defaultLane() +
                            "\n* Create different props for different block materials?" +
                            "\n* [1] * Create different props with different materials " +
                            "\n* [2] * Skip (every prop material will be the same)" +
                            "\n*  *  *" +
                            "\n* [0] * Back \n");

                        string materialInput = Console.ReadLine().Trim();
                        if (materialInput != "1" && materialInput != "2")
                        {
                            Main();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("\n" + defaultLane() +
                                "\n* Starting conversion...");

                            // TRANSFORMING BLOCKS INTO PROPS                    
                            bool differMaterials = materialInput == "1" ? true : false;
                            map = mapEditor.PortMap(map, differMaterials);
                            Console.WriteLine("\n" + defaultLane() +
                                $"\n* Entities (new count): {map.entities.Count()}");                            
                        }
                    }
                    else if (operationInput == "2")
                    {
                        fileName = $"{fileName}_rescaled";
                        /// SELECT NEW SCALE OF THE MAP (for example 1.2)
                        Console.WriteLine("\n" + defaultLane() +
                            "\n* Enter new scale for a map (for example 1.2):");

                        string strScale = Console.ReadLine().Trim();
                        bool success = float.TryParse(strScale, NumberStyles.Float, CultureInfo.CurrentCulture, out float floatScale);
                        if (!success)
                        {
                            success = float.TryParse(strScale, NumberStyles.Float, CultureInfo.InvariantCulture, out floatScale);
                        }
                        if (success)
                        {
                            Console.WriteLine("\n" + defaultLane() +
                                "\n* Starting rescaling...");
                            map = mapEditor.RescaleMap(map, floatScale);
                        }
                        else
                        {
                            Console.WriteLine("\n* [ERROR] Invalid scale value. Returning to main menu.");
                            Main();
                            return;
                        }

                    }
                    // SAVING THE MAP
                    string targetDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DiaboticalRogue\Maps\";
                    if (!Directory.Exists(targetDir))
                    {
                        targetDir = Environment.CurrentDirectory + @"\";
                    }
                    
                    string savePath = Path.Combine(targetDir, $"{fileName}.rbe");
                    mapHandler.SaveMap(map, savePath);

                    FileInfo newFileInfo = new FileInfo(savePath);
                    fileSizeInBytes = newFileInfo.Length;
                    fileSizeInMB = (double)fileSizeInBytes / (1024 * 1024);
                    Console.WriteLine(
                        $"* NEW file size: {fileSizeInMB:F2} MB" + 
                        $"\n******* SUCCESS! *******" +
                        $"\n* Saving new map into Diabotical Rogue appdata folder (into game folder)..." +
                        $"\n* To edit new map type in console: " +
                        $"\n* /edit {fileName}" +
                        "\n" + defaultLane());
                } 
                else
                {
                    Main();
                    return;
                }
                
            }
            else
            {
                Console.WriteLine("* [ERROR] Invalid input. Please enter a valid number corresponding to the map.");
            }

            Console.WriteLine("\n" + defaultLane() + 
                "\n* Select:" +
                "\n* [0]   * Back" +
                "\n* [ESC] * Exit");
            var key = Console.ReadKey();
            if (key.Key != ConsoleKey.Escape)
            {
                Main(); return;
            };
        }

        public static string defaultLane()
        {
            return "************************"; 
        }
        
    }
}