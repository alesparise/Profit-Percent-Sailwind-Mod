using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
using Object = UnityEngine.Object;
using System.ComponentModel;
//poorly written by pr0skynesis (discord username)

namespace ProfitPercent
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ProfitPercentMain : BaseUnityPlugin
    {
        // Necessary plugin info
        public const string pluginGuid = "pr0skynesis.profitpercent";
        public const string pluginName = "Profit Percent";
        public const string pluginVersion = "1.1.0";

        //config file info
        //COLORED TEXT
        public static ConfigEntry<bool> coloredTextConfig;
        public static ConfigEntry<int> greenThresholdConfig;
        public static ConfigEntry<int> blueThresholdConfig;
        //BEST DEALS
        public static ConfigEntry<bool> showBestDealsConfig;
        public void Awake()
        {
            //patching info
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(EconomyUI), "ShowGoodPage");
            MethodInfo patch = AccessTools.Method(typeof(ProfitPercentPatches), "ShowGoodPage_Patch");

            //config file
            //COLORED TEXT
            coloredTextConfig = Config.Bind("A) Colored text", "coloredText", true, "Enables the coloring of the profit in red (loss), green (profit), yellow (neither) and blue (high profit)");
            greenThresholdConfig = Config.Bind("A) Colored text", "greenThreshold", 0, "Sets the value above which the profit % will be colored in green. E.g.: set to 30 to have the text be displayed as green only above 30% profit.");
            blueThresholdConfig = Config.Bind("A) Colored text", "blueThreshold", 100, "Sets the value above which the profit % will be colored in blue, used to identify high profit margins.");
            //BEST DEALS
            showBestDealsConfig = Config.Bind("B) Best Deals", "showBestDeals", true, "Display the best deals from the current port to the destinations in the currently selected bookmark. Set to false to disable.");

            //apply patches
            harmony.Patch(original, new HarmonyMethod(patch));
        }
    }
    public class ProfitPercentPatchesOLD
    {
        static bool instantiated = false;   //flag to check if we already instantiated the new columns
        [HarmonyPostfix]
        public static void ShowGoodPage_Patch(int goodIndex, IslandMarket ___currentIsland, int[][] ___bookmarkIslands, int ___currentBookmark)
        {
            //initialise columns that replace vanilla ones.
            TextMesh modIslandNames = new TextMesh();
            TextMesh modProfitColumn = new TextMesh();
            TextMesh modGoodName = new TextMesh();

            //Initialise new columns
            TextMesh productionText = new TextMesh();
            TextMesh percentText = new TextMesh();
            TextMesh perPoundText = new TextMesh();
            TextMesh highlightBar = new TextMesh();

            //Initialis best deal. This is not a table!
            TextMesh bdBestDeals = new TextMesh();
            TextMesh bdPercent = new TextMesh();
            TextMesh bdPerPound = new TextMesh();
            TextMesh bdAbsolute = new TextMesh();

            //Set up AccessTools info for GetBuyPrice
            MethodInfo buypInfo = AccessTools.Method(typeof(EconomyUI), "GetBuyPrice");
            //Set up AccessTools info for GetSellPrice
            MethodInfo sellpInfo = AccessTools.Method(typeof(EconomyUI), "GetSellPrice");
            
            ShipItem good = PrefabsDirectory.instance.GetGood(goodIndex);
            Good component = good.GetComponent<Good>();
            
            //target existing columns
            EconomyUI eco = EconomyUI.instance;
            TextMesh[] allText = eco.GetComponentsInChildren<TextMesh>();  //get all TextMesh-es in the EconomyUI
            TextMesh islandNames = allText[2];
            TextMesh buyColumn = allText[3];
            TextMesh goodName = allText[6];
            TextMesh sellColumn = allText[7];
            TextMesh header = allText[8];
            TextMesh profitColumn = allText[9];
            TextMesh daysAgo = allText[11];
            TextMesh tabLines = allText[13];
            TextMesh horizontalLine = allText[14];
            //TextMesh firstVerticalLine = allText[15]; //not used
            TextMesh conversionFees = allText[16];

            //Add the new columns
            if (!instantiated)
            {   //instantiate replacemente columns
                modIslandNames = Object.Instantiate(islandNames, islandNames.transform.parent);
                modIslandNames.name = "modIslandNames"; //allText[18]
                modProfitColumn = Object.Instantiate(profitColumn, profitColumn.transform.parent);
                modProfitColumn.name = "modProfitColumn"; //allText[19]
                modGoodName = Object.Instantiate(goodName, goodName.transform.parent);
                modGoodName.name = "modGoodName";   //allText[20]
                //instantiate additional columns
                productionText = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                productionText.name = "productionText"; //allText[21]
                percentText = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                percentText.name = "percentText";   //allText[22]
                perPoundText = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                perPoundText.name = "perPoundText"; //allText[23]
                //instantiate best deals
                bdBestDeals = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                bdBestDeals.name = "bdBestDeals";   //allText[24]
                bdBestDeals.transform.Rotate(0f, 0f, 15f);
                bdBestDeals.anchor = TextAnchor.MiddleLeft;
                bdPercent = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                bdPercent.name = "bdPercent";   //allText[25]
                bdPercent.anchor = TextAnchor.MiddleLeft;
                bdPerPound = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                bdPerPound.name = "bdPerPound"; //allText[26]
                bdPerPound.anchor = TextAnchor.MiddleLeft;
                bdAbsolute = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                bdAbsolute.name = "bdAbsolute";   //allText[27]
                bdAbsolute.anchor = TextAnchor.MiddleLeft;
                highlightBar = Object.Instantiate(buyColumn, buyColumn.transform.parent);
                highlightBar.name = "highlightBar";   //allText[28]
                highlightBar.anchor = TextAnchor.MiddleLeft; 

                instantiated = true;    //note that we instantiated everything
            }
            else
            {   // if they are already istantiated we have to do this for some reason
                modIslandNames = allText[18];
                modProfitColumn = allText[19];
                modGoodName = allText[20];
                productionText = allText[21];
                percentText = allText[22];
                perPoundText = allText[23];
                bdBestDeals = allText[24];
                bdPercent = allText[25];
                bdPerPound = allText[26];
                bdAbsolute = allText[27];
                highlightBar = allText[28];
            }
            //Check if the new columns are created correctly
            if (modIslandNames == null || modProfitColumn == null || productionText == null || percentText == null || perPoundText == null || bdBestDeals == null || bdPercent == null || bdPerPound == null || bdAbsolute == null)
            {
                Debug.LogError("ProfitPercent: Some of the new TextMeshes are null!");
                return;
            }
            else
            {
                modIslandNames.text = "";
                modProfitColumn.text = "";
                modGoodName.text = char.ToUpper(good.name[0]) + good.name.Substring(1);
                productionText.text = "";
                percentText.text = "";
                perPoundText.text = "";
                bdBestDeals.text = "<color=#4D0000>★ Best Deals! ★</color>";
                bdPercent.text = BestDeals(eco, ___currentIsland, ___bookmarkIslands, ___currentBookmark, 0);
                bdPerPound.text = BestDeals(eco, ___currentIsland, ___bookmarkIslands, ___currentBookmark, 1);
                bdAbsolute.text = BestDeals(eco, ___currentIsland, ___bookmarkIslands, ___currentBookmark, 2);
                highlightBar.text = "┌───────────────────────────────────┐\n└───────────────────────────────────┘";
            }
            //CREATE THE UI
            for (int i = 0; i < ___bookmarkIslands[___currentBookmark].Length; i++)
            {   //Assign values to the new columns
                int tempPort = ___bookmarkIslands[___currentBookmark][i];
                //parameters for the AccessTools invoked methods:
                object[] buypParameters = new object[] { ___currentIsland.GetPortIndex(), goodIndex };
                object[] sellpParameters = new object[] { tempPort, goodIndex };

                //Production
                FieldInfo producedGoodPrefabsInfo = AccessTools.Field(typeof(Port), "producedGoodPrefabs");
                GameObject[] producedGoodPrefabs = (GameObject[])producedGoodPrefabsInfo.GetValue(Port.ports[tempPort]);
                int[] producedGoodIndices = producedGoodPrefabs.Select(go => go.GetComponent<SaveablePrefab>().prefabIndex).ToArray();
                int goodPrefabIndex = PrefabsDirectory.GoodToItemIndex(goodIndex);
                if (producedGoodIndices.Contains(goodPrefabIndex))
                {
                    productionText.text += $"✓\n";
                }
                else
                {
                    productionText.text += $"- \n";
                }
                //profit %
                int buyp = (int)buypInfo.Invoke(eco, buypParameters);
                int sellp = (int)sellpInfo.Invoke(eco, sellpParameters);
                float profitPercent = Mathf.Round(((float)(sellp - buyp) / buyp) * 100f);
                
                //profit per pound
                float cargoWeight = component.GetCargoWeight();
                float profitPerPound = (float)Math.Round((sellp - buyp) / cargoWeight, 2);
                //write the columns
                if (ProfitPercentMain.coloredTextConfig.Value)
                {   //apply the color if needed
                    int higherThreshold = ProfitPercentMain.blueThresholdConfig.Value;
                    int lowerThreshold = ProfitPercentMain.greenThresholdConfig.Value;
                    if(ProfitPercentMain.blueThresholdConfig.Value < ProfitPercentMain.greenThresholdConfig.Value)
                    {   //make sure the thresholds are used correctly by switching them around if necessary
                        higherThreshold = ProfitPercentMain.greenThresholdConfig.Value;
                        lowerThreshold = ProfitPercentMain.blueThresholdConfig.Value;
                    }
                    if(float.IsInfinity(profitPercent))
                    {   //if buyp is zero (good not sold in the current port), we get infinite profit. in that case we add a yellow -.
                        modProfitColumn.text += $"<color=#CC7F00>- </color>\n";
                        percentText.text += $"<color=#CC7F00>- </color>\n";
                        perPoundText.text += $"<color=#CC7F00>- </color>\n";
                    }
                    else if (profitPercent > higherThreshold)
                    {   //blue color #051139
                        modProfitColumn.text += $"<color=#051139>{sellp - buyp}</color>\n";
                        percentText.text += $"<color=#051139>{profitPercent}<size=40%>%</size></color>\n";
                        perPoundText.text += $"<color=#051139>{profitPerPound}</color>\n";
                    }
                    else if (profitPercent > lowerThreshold)
                    {   //green color #003300 // old color was #113905
                        modProfitColumn.text += $"<color=#003300>{sellp - buyp}</color>\n";
                        percentText.text += $"<color=#003300>{profitPercent}<size=40%>%</size></color>\n";
                        perPoundText.text += $"<color=#003300>{profitPerPound}</color>\n";
                    }
                    else if (profitPercent < 0f)
                    {   //red color #4D0000 //old color was #7C0000
                        modProfitColumn.text += $"<color=#4D0000>{sellp - buyp}</color>\n";
                        percentText.text += $"<color=#4D0000>{profitPercent}<size=40%>%</size></color>\n";
                        perPoundText.text += $"<color=#4D0000>{profitPerPound}</color>\n";
                    }
                    else // between the green threshold and zero color is yellow
                    {   //yellow color #CC7F00
                        modProfitColumn.text += $"<color=#CC7F00>{sellp - buyp}</color>\n";
                        percentText.text += $"<color=#CC7F00>{profitPercent}<size=40%>%</size></color>\n";
                        perPoundText.text += $"<color=#CC7F00>{profitPerPound}</color>\n";
                    }
                    //island list color
                    if (tempPort == ___currentIsland.GetPortIndex())
                    {   //it's the current island
                        modIslandNames.text += $"<color=#4D0000>• {Port.ports[tempPort].GetPortName()}</color>\n";
                    }
                    else
                    {   //not the currentIsland
                        modIslandNames.text += $"{Port.ports[tempPort].GetPortName()}\n";
                    }
                }
                else
                {   //no colors here
                    if (tempPort == ___currentIsland.GetPortIndex())
                    {   //still add the • if it's the currentIsland
                        modIslandNames.text += $"• {Port.ports[tempPort].GetPortName()}\n";
                    }
                    else
                    {
                        modIslandNames.text += $"{Port.ports[tempPort].GetPortName()}\n";
                    }
                    modProfitColumn.text += $"{sellp - buyp}\n";
                    percentText.text += $"{profitPercent}<size=40%>%</size>\n";
                    perPoundText.text += $"{profitPerPound}\n";
                }
            }

            //TABLE
            //Edit Table
            header.characterSize = 0.85f;
            header.text = "days ago  P.    buy     sell     profit      %    p. pound"; //✓ works!!! Use this for production.
            tabLines.text = "        |   |       |       |        |       |";
            
            //Resize text (more stuff to show, means less space to do so...)
            float charSize = 0.8f;
            modIslandNames.characterSize = charSize;
            modProfitColumn.characterSize = charSize;
            islandNames.characterSize = charSize;
            daysAgo.characterSize = charSize;
            productionText.characterSize = charSize;
            buyColumn.characterSize = charSize;
            sellColumn.characterSize = charSize;
            profitColumn.characterSize = charSize;
            percentText.characterSize = charSize;
            perPoundText.characterSize = charSize;
            conversionFees.characterSize = charSize;

            //Edit column spacing
            MoveX(modIslandNames.transform, 0.23f); //✓
            MoveX(modProfitColumn.transform, -0.45f); //✓
            MoveX(goodName.transform, 100f); //✓     //yeet it away to disable it
            MoveX(islandNames.transform, 100f); //✓     //yeet it away to disable it
            MoveX(daysAgo.transform, 0.133f);   //CHECK WHEN THERE ARE MULTIPLE DIGITS...
            MoveX(productionText.transform, 0.050f); //✓
            MoveX(buyColumn.transform, -0.11f); //✓
            MoveX(sellColumn.transform, -0.27f); //✓
            MoveX(profitColumn.transform, 100f); //✓    //yeet it away to disable it
            MoveX(percentText.transform, -0.61f); //✓
            MoveX(perPoundText.transform, -0.78f);  //✓
            MoveX(horizontalLine.transform, -0.94f); //✓
            //move conversionFees warning text
            MoveXY(conversionFees.transform, 0.64f,-0.8f);

            //BEST DEALS
            //Resize best deals
            bdBestDeals.characterSize = 1.1f;
            bdPercent.characterSize = 0.7f;
            bdPerPound.characterSize = 0.7f;
            bdAbsolute.characterSize = 0.7f;
            
            //Move best deals
            MoveXY(bdBestDeals.transform, 0.74f, -0.15f);   //✓
            MoveXY(bdPercent.transform, 0.64f, -0.20f);     //✓
            MoveXY(bdPerPound.transform, 0.64f, -0.25f);    //✓
            MoveXY(bdAbsolute.transform, 0.64f, -0.30f);    //✓
            if (!ProfitPercentMain.showBestDealsConfig.Value)
            {   //yeet them away if not needed
                MoveX(bdBestDeals.transform, 100f);
                MoveX(bdPercent.transform, 100f);
                MoveX(bdPerPound.transform, 100f);
                MoveX(bdAbsolute.transform, 100f);
            }

            //Move the highlightBar to the correct position:
            highlightBar.lineSpacing = 0.9f;
            highlightBar.color = new Color(0.25f, 0f, 0f);
            float[] yPos = { 0.622f, 0.549f, 0.476f, 0.401f, 0.328f, 0.255f, 0.182f };  //✓ //pick the position based on the currentIsland
            MoveXY(highlightBar.transform, 0.64f, yPos[GetUIPortIndex(___currentIsland.GetPortIndex())]);
            //MoveXY(highlightBar.transform, 0.64f, yPos[goodIndex - 1]); //DEBUG this changes the highlight position based on the good selected, salmon is 0.
            //is the currentIsland in the currentBookmark?
            if(BookmarkCheck(___currentIsland.GetPortIndex()) != ___currentBookmark)
            {
                MoveX(highlightBar.transform, 100f); //yeet it away to disable
            }
            //disable the vanilla islandHighlightBar
            FieldInfo islandHighlightBarInfo = AccessTools.Field(typeof(EconomyUI), "islandHighlightBar");
            Transform islandHighlightBar = (Transform)islandHighlightBarInfo.GetValue(eco);
            MoveX(islandHighlightBar.transform, 100f);  //yeet the vanilla bar out of the frame;

            //DEBUG POSITIONS
            /*Debug.LogWarning("ProfitPercent: ==== ==== Positions ==== ====");
            Debug.LogWarning($"ProfitPercent: header position: {header.transform.localPosition}, line spacing: {header.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: tabLines position: {tabLines.transform.localPosition}, line spacing: {tabLines.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: islandNames position: {islandNames.transform.localPosition}, line spacing: {islandNames.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: daysAgo position: {daysAgo.transform.localPosition}, line spacing: {daysAgo.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: productionText position: {productionText.transform.localPosition}, line spacing: {productionText.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: buyColumn position: {buyColumn.transform.localPosition}, line spacing: {buyColumn.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: sellColumn position: {sellColumn.transform.localPosition}, line spacing: {sellColumn.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: profitColumn position: {profitColumn.transform.localPosition}, line spacing: {profitColumn.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: percentText position: {percentText.transform.localPosition}, line spacing: {percentText.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: perPoundText position: {perPoundText.transform.localPosition},line spacing: {perPoundText.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: horizontalLine position: {horizontalLine.transform.localPosition},line spacing: {horizontalLine.lineSpacing}");
            Debug.LogWarning($"ProfitPercent: bdBestDeals position: {bdBestDeals.transform.localPosition}");
            Debug.LogWarning($"ProfitPercent: bdPercent position: {bdPercent.transform.localPosition}");
            Debug.LogWarning($"ProfitPercent: bdPerPound position: {bdPerPound.transform.localPosition}");
            Debug.LogWarning($"ProfitPercent: bdAbsolute position: {bdAbsolute.transform.localPosition}");
            Debug.LogWarning($"ProfitPercent: highlightBar position: {highlightBar.transform.localPosition}, line spacing: {highlightBar.lineSpacing}");
            */
            
        }
        private static void MoveX(Transform transform, float x)
        {   //give this a transform and the new value for the transform.localPosition x coordinate
            //It's not an offset from the original position! It moves it to the new x value!!!
            //larger value → move left
            Vector3 v = transform.localPosition;
            v.x = x;
            transform.localPosition = v;
        }
        private static void MoveXY(Transform transform, float x, float y)
        {   //give this a transform and the new value for the transform.localPosition x coordinate
            //It's not an offset from the original position! It moves it to the new x value!!!
            //larger value → move left, up
            Vector3 v = transform.localPosition;
            v.x = x;
            v.y = y;
            transform.localPosition = v;
        }
        private static string BestDeals(EconomyUI eco, IslandMarket currentIsland, int[][] bookmarkIslands, int currentBookmark, int selector)
        {   //takes in the currentIsland, currentBookmark and a value of 0,1 or 2 (0 = percent, 1 = perpound, 2 = perunit)
            //returns a string telling what the best deal is (%, p.p. or p.u.) and to what port.
            if (!ProfitPercentMain.showBestDealsConfig.Value)
            {   //avoid doing all the calculations if unnecessary
                return "";
            }
            //Set up AccessTools Info
            MethodInfo buypInfo = AccessTools.Method(typeof(EconomyUI), "GetBuyPrice");
            MethodInfo sellpInfo = AccessTools.Method(typeof(EconomyUI), "GetSellPrice");
            
            //Create the goods array
            GameObject[] directory = PrefabsDirectory.instance.directory;   //get the directory of all the prefabs
            TransactionCategory[] validCategories = { TransactionCategory.bulkFood, TransactionCategory.bulkWater, TransactionCategory.bulkAlco, TransactionCategory.bulkGood };   //valid goods category  
            ShipItem[] goods = directory    //using LINQ we create the array
            .Where(go => go != null) // Filter out null entries
            .Select(go => go.GetComponent<ShipItem>()) // Get the ShipItem component
            .Where(shipItem => shipItem != null && validCategories.Contains(shipItem.category)) // Check for valid ShipItem and category
            .ToArray(); // Convert to array

            int[] buyPrices = new int[goods.Length];     // create an array for the buy prices from the current island
            //actual values used in the calculations
            int[] profitArray = new int[goods.Length];   // create an array for the profits which we'll use for each island in the bookmark
            float percent;
            float perPound;
            //store the indexes of the best ports
            int bestPercentPort = 0;
            int bestPerPoundPort = 0;
            int bestPort = 0;
            //store the indexes of the best goods
            int bestPercentGood = 0;
            int bestPerPoundGood = 0;
            int bestGood = 0;
            //store the best profits values
            float bestPercent = -1000f;
            float bestPerPound = -1000f;
            int bestProfit = -1000;

            for (int i = 0; i < buyPrices.Length; i++)
            {   //iterate through all the goods available
                int goodIndex = i + 1;
                if(i >= 44)
                {   //skips the blank good between cave and forest mushrooms
                    goodIndex++;
                }
                object[] buypParameters= { currentIsland.GetPortIndex(), goodIndex };
                buyPrices[i] = (int)buypInfo.Invoke(eco, buypParameters);

                for (int j = 0; j < bookmarkIslands[currentBookmark].Length; j++)
                {   //this iterates through the islands in the bookmark
                    int tempPort = bookmarkIslands[currentBookmark][j];
                    for (int k = 0; k < profitArray.Length; k++)
                    {   //we got to iterate through all the goods again
                        int goodIndex2 = k + 1;
                        if (k >= 44)
                        {   //skips the blank good between cave and forest mushrooms
                            goodIndex2++;
                        }
                        float cargoWeight = goods[k].GetComponent<Good>().GetCargoWeight();
                        object[] sellpParameters= { tempPort, goodIndex2 };
                        int sellp = (int)sellpInfo.Invoke(eco, sellpParameters);
                        if (buyPrices[k] == 0)
                        {   //if buy price is zero, the good is not sold, should not be considered a good deal!
                            percent = -10000f;
                            perPound = -10000f;
                            profitArray[k] = -10000;
                        }
                        else
                        {
                            percent = Mathf.Round(((float)(sellp - buyPrices[k]) / buyPrices[k]) * 100f);
                            perPound = (float)Math.Round((sellp - buyPrices[k]) / cargoWeight, 2);
                            profitArray[k] = sellp - buyPrices[k];
                        }
                        if (percent > bestPercent)
                        {
                            bestPercentGood = k;
                            bestPercent = percent;
                            bestPercentPort = j;
                        }
                        if (perPound > bestPerPound)
                        {
                            bestPerPoundGood = k;
                            bestPerPound = perPound;
                            bestPerPoundPort = j;
                        }
                        if (profitArray[k] > bestProfit)
                        {
                            bestGood = k;
                            bestProfit = profitArray[k];
                            bestPort = j;
                        }
                    }
                }
            }
            if(selector == 0)
            {   //we want the best percent profit
                string goodName = goods[bestPercentGood].name;
                string portName = Port.ports[FixPortIndex(bestPercentPort, currentBookmark)].GetPortName();
                string value = $"{bestPercent}%";
                goodName = char.ToUpper(goodName[0]) + goodName.Substring(1);
                return $"• {goodName} to {portName} will return a profit of {value}!";
            }
            if(selector == 1)
            {   //we want the best per pound profit
                string goodName = goods[bestPerPoundGood].name;
                string portName = Port.ports[FixPortIndex(bestPerPoundPort, currentBookmark)].GetPortName();
                string value = $"{bestPerPound} per pound";
                goodName = char.ToUpper(goodName[0]) + goodName.Substring(1);
                return $"• {goodName} to {portName} will return a profit of {value}!";
            }
            if (selector == 2)
            {   //we want the best per unit profit
                string goodName = goods[bestGood].name;
                string portName = Port.ports[FixPortIndex(bestPort, currentBookmark)].GetPortName();
                string value = $"{bestProfit} per unit";
                goodName = char.ToUpper(goodName[0]) + goodName.Substring(1);
                return $"• {goodName} to {portName} will return a profit of {value}!";
            }

            return "Something is wrong in this place...";
        }
        private static int FixPortIndex(int index, int currentBookmark)
        {   //fixes the index so that the GetPortName() can work properly.
            if(currentBookmark == 0)
            {
                int[] portIndex = { 0, 1, 2, 3, 4, 5, 6 };
                return portIndex[index];
            }
            else if (currentBookmark == 1)
            {
                int[] portIndex = { 9, 10, 11, 12, 13, 14 };
                return portIndex[index];
            }
            else if (currentBookmark == 2)
            {
                int[] portIndex = { 15, 16, 17, 18, 19, 20 };
                return portIndex[index];
            }
            else
            {
                int[] portIndex = { 22, 23, 24, 25 };
                return portIndex[index];
            }
        }
        private static int GetUIPortIndex(int portIndex)
        {   //normalizes the portIndex to values 0-6, so that the highlightBar can work properly.
            if (portIndex <= 6)
            {   //Al'Ankh
                return portIndex;
            }
            if (portIndex >= 9 && portIndex <= 14)
            {   //Emerald
                return portIndex - 9;
            }
            if (portIndex >= 15 && portIndex <= 20)
            {   //Aestrin
                return portIndex - 15;
            }
            if (portIndex >= 22 && portIndex <= 25)
            {   //Lagoon
                return portIndex - 22;
            }
            if (portIndex == 21)
            {   //Chronos (might have to change this)
                return 0;
            }
            else
            {
                return 0;
            }
        }
        private static int BookmarkCheck(int portIndex)
        {   //checks if the currentIsland (portIndex) is part of the currentBookmark
            int[] alhank = { 0, 1, 2, 3, 4, 5, 6 }; //indexes for Al'Ankh
            int[] emerald = { 9, 10, 11, 12, 13, 14 }; //indexes for Emerald
            int[] aestrin = { 15, 16, 17, 18, 19, 20 }; //indexes for Aestrin
            int[] lagoon = { 22, 23, 24, 25 }; //indexes for Lagoon

            if (alhank.Contains(portIndex))
            {
                return 0;
            }
            else if (emerald.Contains(portIndex))
            {
                return 1;
            }
            else if (aestrin.Contains(portIndex))
            {
                return 2;
            }
            else if (lagoon.Contains(portIndex))
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }
    }
}
