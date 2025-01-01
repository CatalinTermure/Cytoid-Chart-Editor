package com.chovvy.unityfileutils;

import android.app.Activity;
import android.app.DownloadManager;
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

import androidx.core.content.FileProvider;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Arrays;


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
            assert inputStream != null;
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
            return e + "\n" + Arrays.toString(e.getStackTrace());
        }
    }

    public static String ExportCytoidLevel(Context context, String filePath) {
        File localFile = new File(filePath);

        try {
            ExportCytoidLevelToDownloads(context, filePath, localFile);
        } catch (Exception e) {
            return e + "\n" + Arrays.toString(e.getStackTrace());
        }

        return "";
    }

    public static String ExportToCytoid(Context context, String filePath) {
        File localFile = new File(filePath);
        Uri contentUri = FileProvider.getUriForFile(context, "com.chovvy.fileprovider", localFile);
        if (contentUri == null) return "Could not get content uri";

        Intent intent = new Intent();
        intent.setAction(Intent.ACTION_VIEW);
        intent.setDataAndType(contentUri, "application/octet-stream");
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        intent.addFlags(Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
        context.startActivity(intent);
        return "";
    }

    private static void ExportCytoidLevelToDownloads(Context context, String localFilePath, File localFile) throws IOException {
        if (Build.VERSION.SDK_INT <= Build.VERSION_CODES.Q) {
            // save to downloads
            DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
            downloadManager.enqueue(new DownloadManager.Request(Uri.fromFile(localFile)));
            return;
        }
        ContentValues contentValues = new ContentValues();
        contentValues.put(MediaStore.MediaColumns.DISPLAY_NAME, localFile.getName());
        contentValues.put(MediaStore.MediaColumns.MIME_TYPE, "application/octet-stream");
        contentValues.put(MediaStore.MediaColumns.SIZE, localFile.length());

        ContentResolver resolver = context.getContentResolver();
        Uri destinationFileUri = resolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, contentValues);
        assert destinationFileUri != null;
        OutputStream output = resolver.openOutputStream(destinationFileUri);
        InputStream input = new FileInputStream(localFile);
        CopyFile(input, output);
    }

    private static void CopyFile(InputStream input, OutputStream output) throws IOException {
        byte[] buf = new byte[1024];
        int len;
        while ((len = input.read(buf)) > 0) {
            output.write(buf, 0, len);
        }
    }
}
