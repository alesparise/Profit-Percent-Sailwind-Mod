**Features:**  
• Adds new columns to the trade book showing percentage profit, per pound profit and whether a good is produced(✓) or not(-) in a certain port;  
• Colors the profit (and the percentage) in green if its positive, red in negative and yellow if zero. Very high profits (above 100% by default) are colored in blue;  
• Changes the highlight bar that shows the current island;
• Capitalizes the goods name (salmon → Salmon);
• Adds a section about Best Deals from the current island to destinations in the currently selected archipelago.
• Configuration options inlcude:  
	• Enable/disable colored text in the UI;  
	• Change the threshold for the green text and blue text (you can set the percentage at which text becomes green or blue);  
	• Enable/disable Best Deals section;    
**Requirements: Requires BepInEx**  
**Installation:** Download ProfitPercent.dll and move it into the *...\Sailwind\BepInEx\plugins folder*  
  
**Game version:** *0.26*  
**Mod Version:** *1.1.0*  
**Warning:** I don't really know much about programming so this might be buggy. I tested it for a while and didn't have any issue, but please let me know if there are problems. I'd recommend making a backup save, just in case, but I don't think this would break a savegame.  
**Compatibility:** This mod replaces the GetProfitString method in the EconomyUI class. If another mod changes that there will be issues, I suppose. If that method or class get changed in the future the mod might stop working, but I'd say for the foreseeable future it should work for a few updates.  
  
**Changelog: v1.1.0**  
• Code redone entirely;  
• Changed the trade book table to add in the new informations;  
• Added % column, p.pound column and production column;  
• Added Best Deals section;  
• Added configuration options;  
• Capitalized the selected good name.  