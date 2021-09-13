package com.chovvy.unityfileutils;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
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

    public static final FileUtils ourInstance = new FileUtils();
    public static FileUtils getInstance() { return ourInstance; }

    public String GetCytoidStorageDirectory() {
        File cytoidDir = new File(Environment.getExternalStorageDirectory(), "Cytoid");
        return cytoidDir.getAbsolutePath();
    }

    public void ExportCytoidLevel(Context context, String localFilePath) {
        File localFile = new File(localFilePath);
        Intent launchIntent = context.getPackageManager().getLaunchIntentForPackage("me.tigerhix.cytoid");

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

                        if(launchIntent == null) return;
                        launchIntent.setData(destinationFileUri);
                        launchIntent.setFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
                        context.startActivity(launchIntent);
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

            File cytoidExportedFile = new File(GetCytoidStorageDirectory(), localFile.getName());

            boolean success = true;

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
                    success = false;
                } catch (SecurityException e) {
                    Log.e(LOGTAG, "No permission to open the external file. Path: " + destinationFile.getAbsolutePath());
                    success = false;
                }
            } catch(FileNotFoundException e) {
                Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
                success = false;
            } catch(SecurityException e) {
                Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
                success = false;
            } catch (IOException e) {
                e.printStackTrace();
                success = false;
            }

            if(!success) return;

            try {
                try (InputStream input = new FileInputStream(localFile)) {
                    try (OutputStream output = new FileOutputStream(cytoidExportedFile)) {
                        byte[] buf = new byte[1024];
                        int len;
                        while ((len = input.read(buf)) > 0) {
                            output.write(buf, 0, len);
                        }
                    }
                } catch (FileNotFoundException e) {
                    Log.e(LOGTAG, "Could not find output file. Path: " + cytoidExportedFile.getAbsolutePath());
                    success = false;
                } catch (SecurityException e) {
                    Log.e(LOGTAG, "No permission to open the external file. Path: " + cytoidExportedFile.getAbsolutePath());
                    success = false;
                }
            } catch(FileNotFoundException e) {
                Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
                success = false;
            } catch(SecurityException e) {
                Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
                success = false;
            } catch (IOException e) {
                e.printStackTrace();
                success = false;
            }

            if(!success) return;

            if(launchIntent == null) return;

            context.startActivity(launchIntent);
        }
    }
}
