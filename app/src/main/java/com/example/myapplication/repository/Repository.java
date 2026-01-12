package com.example.myapplication.repository;

import android.content.Context;
import androidx.annotation.Nullable;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.utils.ErrorMessageHelper;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class Repository {

    protected final ApiService apiService;
    protected final Context context;

    public Repository(Context context) {
        this.context = context.getApplicationContext();
        this.apiService = RetrofitClient.getApiService();
    }

    protected <T> void executeCall(Call<ApiResponse<T>> call, RepositoryCallback<T> callback) {
        call.enqueue(new Callback<ApiResponse<T>>() {
            @Override
            public void onResponse(Call<ApiResponse<T>> call, Response<ApiResponse<T>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<T> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        callback.onSuccess(apiResponse.getData());
                    } else {
                        String errorMsg = apiResponse.getError() != null
                            ? apiResponse.getError().getMessage()
                            : "Unknown error";
                        callback.onError(errorMsg);
                    }
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(
                        context, response.code(), "Request failed");
                    callback.onError(errorMsg);
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<T>> call, Throwable t) {
                callback.onError("Vérifiez votre connexion internet");
            }
        });
    }

    public interface RepositoryCallback<T> {
        void onSuccess(T data);
        void onError(String message);
    }
}
