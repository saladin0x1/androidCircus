package com.example.myapplication.repository;

import android.content.Context;
import com.example.myapplication.api.models.LoginRequest;
import com.example.myapplication.api.models.LoginResponse;
import com.example.myapplication.api.models.ForgotPasswordRequest;
import com.example.myapplication.api.models.ResetPasswordRequest;
import com.example.myapplication.api.models.RegisterRequest;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class AuthRepository extends Repository {

    private static AuthRepository instance;

    private AuthRepository(Context context) {
        super(context);
    }

    public static synchronized AuthRepository getInstance(Context context) {
        if (instance == null) {
            instance = new AuthRepository(context);
        }
        return instance;
    }

    public void login(String email, String password, RepositoryCallback<LoginResponse.Data> callback) {
        LoginRequest request = new LoginRequest(email, password);
        apiService.login(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    LoginResponse loginResponse = response.body();
                    if (loginResponse.isSuccess()) {
                        callback.onSuccess(loginResponse.getData());
                    } else {
                        callback.onError(loginResponse.getError() != null ? loginResponse.getError() : "Login failed");
                    }
                } else {
                    callback.onError("Échec de la connexion");
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                callback.onError("Vérifiez votre connexion internet");
            }
        });
    }

    public void register(String email, String password, String firstName, String lastName,
                         String role, RepositoryCallback<LoginResponse.Data> callback) {
        // Convert string role to int: Patient=0, Doctor=1, Clerk=2
        int roleInt = 0; // default Patient
        if ("Doctor".equalsIgnoreCase(role)) roleInt = 1;
        else if ("Clerk".equalsIgnoreCase(role)) roleInt = 2;

        RegisterRequest request = new RegisterRequest(firstName, lastName, email, password, "", roleInt);
        // Set optional fields if needed
        if (roleInt == 0) {
            request.setDateOfBirth("2000-01-01"); // Default DOB for patients
        }

        apiService.register(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    LoginResponse loginResponse = response.body();
                    if (loginResponse.isSuccess()) {
                        callback.onSuccess(loginResponse.getData());
                    } else {
                        callback.onError(loginResponse.getError() != null ? loginResponse.getError() : "Registration failed");
                    }
                } else {
                    callback.onError("Échec de l'inscription");
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                callback.onError("Vérifiez votre connexion internet");
            }
        });
    }

    public void forgotPassword(String email, final RepositoryCallback<Void> callback) {
        ForgotPasswordRequest request = new ForgotPasswordRequest(email);
        executeCall(apiService.forgotPassword(request), new RepositoryCallback<Object>() {
            @Override
            public void onSuccess(Object data) {
                callback.onSuccess(null);
            }

            @Override
            public void onError(String message) {
                callback.onError(message);
            }
        });
    }

    public void resetPassword(String email, String newPassword, String confirmPassword,
                             final RepositoryCallback<Void> callback) {
        ResetPasswordRequest request = new ResetPasswordRequest(email, newPassword, confirmPassword);
        executeCall(apiService.resetPassword(request), new RepositoryCallback<Object>() {
            @Override
            public void onSuccess(Object data) {
                callback.onSuccess(null);
            }

            @Override
            public void onError(String message) {
                callback.onError(message);
            }
        });
    }

    public void refreshToken(final RepositoryCallback<LoginResponse.Data> callback) {
        apiService.refreshToken(new com.example.myapplication.api.models.RefreshTokenRequest())
                .enqueue(new Callback<LoginResponse>() {
                    @Override
                    public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                        if (response.isSuccessful() && response.body() != null) {
                            LoginResponse loginResponse = response.body();
                            if (loginResponse.isSuccess()) {
                                callback.onSuccess(loginResponse.getData());
                            } else {
                                callback.onError(loginResponse.getError() != null ? loginResponse.getError() : "Refresh failed");
                            }
                        } else {
                            callback.onError("Échec du rafraîchissement");
                        }
                    }

                    @Override
                    public void onFailure(Call<LoginResponse> call, Throwable t) {
                        callback.onError("Vérifiez votre connexion internet");
                    }
                });
    }
}
