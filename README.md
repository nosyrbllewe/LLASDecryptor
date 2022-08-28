# LLASDecryptor
Decrypts Assets from the game Love Live School Idol Festival All Stars!

### Requirements
 - Requires .NET 6 Runtime installed. Any recent Windows 10/11 version should have it

### How to Use

1. Download and extract the zip file into a folder
2. Run the LLASDecryptor.exe
3. With the application now open, select the input folder of the files in the first textbox (this should be the 'files' folder from SIFAS)
4. In the second textbox, enter/select the folder where you want the files to be extracted to.
5. Enter the decryption key you got likely got earlier from the PlayerPrefs
6. On the left side, select the tables that you want to decrypt/export.
7. Click on the "Decrypt" button
8. After the progress bars finish, the files should now be decrypted into the chosen folder

### To Do
- Add ability to retrieve only portion of audio files (e.g music or voice lines)
- Rewrite UI to be OS agnostic (e.g Blazor Desktop) to allow function on Mac and Linux 

