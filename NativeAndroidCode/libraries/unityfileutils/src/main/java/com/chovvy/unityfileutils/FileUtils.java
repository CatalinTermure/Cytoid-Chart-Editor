package com.chovvy.unityfileutils;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.MediaStore;
import android.provider.OpenableColumns;
import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Arrays;
import java.util.Objects;


/**
 * @noinspection unused
 */
public class FileUtils {
    private static final String LOGTAG = "CCE.AndroidPlugin";

    public static String GetCytoidStorageDirectory() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            return "/storage/emulated/0/Android/data/me.tigerhix.cytoid/files/Cytoid";
        }
        return "/storage/emulated/0/Cytoid";
    }

    private static String GetFileNameFromContentUri(Context context, Uri uri) throws IOException {
        String fileName = null;
        try (Cursor cursor = context.getContentResolver().query(uri, null, null, null, null)) {
            if (cursor != null && cursor.moveToFirst()) {
                int nameIndex = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME);
                return cursor.getString(nameIndex);
            }
        }
        throw new IOException("Could not get file name from uri");
    }

    public static String CopyFileToExternalStorage(Context context, Uri uri) throws IOException {
        ContentResolver resolver = context.getContentResolver();
        InputStream inputStream = resolver.openInputStream(uri);
        String filename = GetFileNameFromContentUri(context, uri);
        File outputFile = new File(context.getExternalFilesDir(null), filename);
        try (OutputStream output = new FileOutputStream(outputFile)) {
            CopyFile(inputStream, output);
        }

        return outputFile.getAbsolutePath();
    }

    public static String HandleIntent(Activity activity) {
        Intent intent = activity.getIntent();
        if (intent == null) return "No intent";

        Uri uri = intent.getData();
        if (uri == null) return "No uri";

        try {
            return CopyFileToExternalStorage(activity, uri);
        } catch (IOException e) {
            return Arrays.toString(e.getStackTrace());
        }
    }

    public static void ExportCytoidLevel(Context context, String localFilePath) {
        File localFile = new File(localFilePath);
        Intent launchIntent = context.getPackageManager().getLaunchIntentForPackage("me.tigerhix.cytoid");

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ExportCytoidLevelMediaStore(context, localFilePath, localFile, launchIntent);
        } else {
            ExportCytoidLevelIntent(context, localFilePath, localFile, launchIntent);
        }
    }

    private static void ExportCytoidLevelMediaStore(Context context, String localFilePath, File localFile, Intent launchIntent) {
        if (Build.VERSION.SDK_INT <= Build.VERSION_CODES.Q) return;
        ContentValues contentValues = new ContentValues();
        contentValues.put(MediaStore.MediaColumns.DISPLAY_NAME, localFile.getName());
        contentValues.put(MediaStore.MediaColumns.MIME_TYPE, "application/zip");
        contentValues.put(MediaStore.MediaColumns.SIZE, localFile.length());

        ContentResolver resolver = context.getContentResolver();
        Uri destinationFileUri = resolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, contentValues);
        try (OutputStream output = resolver.openOutputStream(destinationFileUri);
             InputStream input = new FileInputStream(localFile)) {
            CopyFile(input, output);

            if (launchIntent == null) return;
            launchIntent.setData(destinationFileUri);
            launchIntent.setFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            context.startActivity(launchIntent);
        } catch (FileNotFoundException e) {
            Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
        } catch (SecurityException e) {
            Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
        } catch (Exception e) {
            Log.e(LOGTAG, "Could not export file. Path: " + localFilePath);
        }
    }

    private static void ExportCytoidLevelIntent(Context context, String localFilePath, File localFile, Intent launchIntent) {
        File downloadsDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS);

        File destinationFile = new File(downloadsDir, localFile.getName());

        File cytoidExportedFile = new File(GetCytoidStorageDirectory(), localFile.getName());

        boolean success = true;

        try {
            try (InputStream input = new FileInputStream(localFile);
                 OutputStream output = new FileOutputStream(destinationFile)) {
                CopyFile(input, output);
            } catch (FileNotFoundException e) {
                Log.e(LOGTAG, "Could not find output file. Path: " + destinationFile.getAbsolutePath());
                success = false;
            } catch (SecurityException e) {
                Log.e(LOGTAG, "No permission to open the external file. Path: " + destinationFile.getAbsolutePath());
                success = false;
            }
        } catch (FileNotFoundException e) {
            Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
            success = false;
        } catch (SecurityException e) {
            Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
            success = false;
        } catch (IOException e) {
            e.printStackTrace();
            success = false;
        }

        if (!success) return;

        try {
            try (InputStream input = new FileInputStream(localFile)) {
                try (OutputStream output = new FileOutputStream(cytoidExportedFile)) {
                    CopyFile(input, output);
                }
            } catch (FileNotFoundException e) {
                Log.e(LOGTAG, "Could not find output file. Path: " + cytoidExportedFile.getAbsolutePath());
                success = false;
            } catch (SecurityException e) {
                Log.e(LOGTAG, "No permission to open the external file. Path: " + cytoidExportedFile.getAbsolutePath());
                success = false;
            }
        } catch (FileNotFoundException e) {
            Log.e(LOGTAG, "Could not find input file. Path: " + localFilePath);
            success = false;
        } catch (SecurityException e) {
            Log.e(LOGTAG, "No permission to open the local file. Path: " + localFilePath);
            success = false;
        } catch (IOException e) {
            e.printStackTrace();
            success = false;
        }

        if (!success) return;

        if (launchIntent == null) return;

        context.startActivity(launchIntent);
    }

    private static void CopyFile(InputStream input, OutputStream output) throws IOException {
        byte[] buf = new byte[1024];
        int len;
        while ((len = input.read(buf)) > 0) {
            output.write(buf, 0, len);
        }
    }
}
