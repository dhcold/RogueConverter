using System.Globalization;
using System.Numerics;
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
                string operationInput = Console.ReadLine().Trim();

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
                            "\n* Reminder: Each block in Diabotical is 40u*20u*40u" + 
                            "\n* Enter new scale [XYZ] for a map (either 1 value or 3: '1,2' or '1*1.2*3'):");

                        string strScale = Console.ReadLine().Trim();
                        Vector3 scaleVector = ParseVector3(strScale);
                        if (scaleVector != Vector3.Zero)
                        {
                            Console.WriteLine("\n" + defaultLane() +
                                $"\n* New scale for the map: ({scaleVector.X}; {scaleVector.Y}; {scaleVector.Z})" +
                                "\n* Starting rescaling...");
                            map = mapEditor.RescaleMap(map, scaleVector);
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
        static Vector3 ParseVector3(string input)
        {
            // Replace common delimiters with spaces for easy splitting
            input = input.Replace(';', ' ').Replace('\t', ' ').Replace('*', ' ').Replace('x', ' ').Replace('-', ' ').Replace('_', ' ');

            // Split input by spaces
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Parse as single value or as three values
            if (parts.Length == 1 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.CurrentCulture, out float singleValue))
            {
                return new Vector3(singleValue, singleValue, singleValue);
            }
            else if (parts.Length == 3 &&
                TryParseFloat(parts[0], out float x) &&
                TryParseFloat(parts[1], out float y) &&
                TryParseFloat(parts[2], out float z))
            {
                return new Vector3(x, y, z);
            }

            // Return Vector3.Zero in case of an invalid input
            return Vector3.Zero;
        }

        static bool TryParseFloat(string input, out float result)
        {
            // Try parsing using the current culture
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
            {
                return true;
            }

            // If it fails, try parsing using the invariant culture
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }

            return false;
        }
    }
}