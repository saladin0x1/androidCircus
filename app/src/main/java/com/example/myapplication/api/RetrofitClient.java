package com.example.myapplication.api;

import android.content.Context;
import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import java.util.concurrent.TimeUnit;

public class RetrofitClient {

    // UNIVERSAL CONFIGURATION:
    // Use your machine's LAN IP address here.
    // Works for: Emulator, Real Device (WiFi), and prepares for VPS IP.
    private static final String BASE_URL = "http://192.168.1.189:5000/api/";

    private static Retrofit retrofit = null;
    private static Context appContext = null;

    public static void init(Context context) {
        if (appContext == null) {
            appContext = context.getApplicationContext();
        }
    }

    public static Retrofit getClient() {
        if (retrofit == null) {
            HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
            logging.setLevel(HttpLoggingInterceptor.Level.BODY);

            OkHttpClient.Builder clientBuilder = new OkHttpClient.Builder()
                    .addInterceptor(logging)
                    .connectTimeout(30, TimeUnit.SECONDS)
                    .readTimeout(30, TimeUnit.SECONDS)
                    .writeTimeout(30, TimeUnit.SECONDS);

            // Add auth interceptor if context is available
            if (appContext != null) {
                clientBuilder.addInterceptor(new AuthInterceptor(appContext));
            }

            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(clientBuilder.build())
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }

    public static ApiService getApiService() {
        return getClient().create(ApiService.class);
    }
}
