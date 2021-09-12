package com.chovvy.unityfileutils;

import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public class FileUtils {
    private static final String LOGTAG = "CCE.AndroidPlugin";

    public void CopyLocalFileToDownloads(Context context, String localFilePath) {
        File localFile = new File(localFilePath);

        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ContentValues contentValues = new ContentValues();
            contentValues.put(MediaStore.MediaColumns.DISPLAY_NAME, localFile.getName());
            contentValues.put(MediaStore.MediaColumns.MIME_TYPE, "application/zip");
            contentValues.put(MediaStore.MediaColumns.SIZE, localFile.length());

            ContentResolver resolver = context.getContentResolver();
            Uri destinationFileUri = resolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, contentValues);
            try {
                try (OutputStream output = resolver.openOutputStream(destinationFileUri)) {
                    try (InputStream input = new FileInputStream(localFile)) {
                        byte[] buf = new byte[1024];
                        int len;
                        while ((len = input.read(buf)) > 0) {
                            output.write(buf, 0, len);
                        }
                    }
                } catch (FileNotFoundException e) {
                    Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
                } catch (SecurityException e) {
                    Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
                }
            } catch(FileNotFoundException e) {
                Log.e(LOGTAG, "Could not open destination file. " + destinationFileUri.toString());
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        else {
            File downloadsDir =
                    Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS);

            File destinationFile = new File(downloadsDir, localFile.getName());

            try {
                try (InputStream input = new FileInputStream(localFile)) {
                    try (OutputStream output = new FileOutputStream(destinationFile)) {
                        byte[] buf = new byte[1024];
                        int len;
                        while ((len = input.read(buf)) > 0) {
                            output.write(buf, 0, len);
                        }
                    }
                } catch (FileNotFoundException e) {
                    Log.e(LOGTAG, "Could not find output file. Path: " + destinationFile.getAbsolutePath());
                } catch (SecurityException e) {
                    Log.e(LOGTAG, "No permission to open the external file. Path: " + destinationFile.getAbsolutePath());
                }
            } catch(FileNotFoundException e) {
                Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
            } catch(SecurityException e) {
                Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}
