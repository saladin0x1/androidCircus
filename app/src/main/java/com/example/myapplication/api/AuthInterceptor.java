package com.example.myapplication.api;

import android.content.Context;
import java.io.IOException;
import okhttp3.Interceptor;
import okhttp3.Request;
import okhttp3.Response;

public class AuthInterceptor implements Interceptor {

    private final Context context;

    public AuthInterceptor(Context context) {
        this.context = context.getApplicationContext();
    }

    @Override
    public Response intercept(Chain chain) throws IOException {
        Request originalRequest = chain.request();

        // Get token from SessionManager
        SessionManager sessionManager = new SessionManager(context);
        String token = sessionManager.getAuthHeader();

        // Skip auth for auth endpoints
        String path = originalRequest.url().encodedPath();
        if (path.contains("/auth/login") || path.contains("/auth/register") ||
            path.contains("/auth/forgot-password") || path.contains("/auth/reset-password")) {
            return chain.proceed(originalRequest);
        }

        // Add auth header if token exists
        if (token != null) {
            Request authenticatedRequest = originalRequest.newBuilder()
                    .header("Authorization", token)
                    .build();
            return chain.proceed(authenticatedRequest);
        }

        return chain.proceed(originalRequest);
    }
}
