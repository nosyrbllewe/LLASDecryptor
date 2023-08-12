# Love Live All Stars! Decryptor
Decrypts Assets from the game Love Live School Idol Festival All Stars!

This should work on both the Japanese and Global servers of the game.

## Requirements
- Requires installation of the .NET 6 runtime. This should already be pre-installed on any recent version of Windows 10 or 11.

### How to Use

1. Get the SIFAS data files if you haven't already (see below).
2. Download and extract the zip file into a folder
3. Run the LLASDecryptor.exe
4. With the application now open, select the "Input" folder of the files in the first textbox (this should be the 'files' folder from SIFAS and should have the database files directly inside it).
5. In the second textbox, enter/select the "Output" folder where you want the files to be extracted to.
6. Enter the decryption key you got earlier from the PlayerPrefs
7. On the left side, select the tables that you want to decrypt/export.
8. Click on the "Decrypt" button
9. After the progress bars finish, the files should now be decrypted into the chosen folder
10. Use [AssetStudio](https://github.com/Perfare/AssetStudio) to open and extract the decrypted *.unity files.

### Getting Data Files
1. Turn on SIFAS and download all of the files
2. Get the unique decryption key from PlayerPrefs file (likely requires root). It is labeled as "SQ" in the XML file. Fortunately, this should only need to be done once per installation. The path is likely similar to "data/data/com.klab.lovelive.allstars/sharedprefs/com.klab.lovelive.allstars.v2.playerprefs.xml"
3.  Copy the folder from "/sdcard/Android/data/com.klab.lovelive.allstars/files/files" containing the downloaded files to your computer. This is the "Input" folder from above.

### To Do
- Rewrite UI to be OS agnostic (e.g Blazor Desktop or Avalonia) to allow function on Mac and Linux
- Allow getting specific textures and organizing them accordingly (e.g. cards, backgrounds, or emblems)

