package com.example.myapplication.repository;

import android.content.Context;
import com.example.myapplication.api.models.UpdatePasswordRequest;
import com.example.myapplication.api.models.UpdateProfileRequest;
import com.example.myapplication.api.models.UserProfile;
import com.example.myapplication.api.models.PendingUser;
import java.util.List;

public class UserRepository extends Repository {

    private static UserRepository instance;

    private UserRepository(Context context) {
        super(context);
    }

    public static synchronized UserRepository getInstance(Context context) {
        if (instance == null) {
            instance = new UserRepository(context);
        }
        return instance;
    }

    public void getProfile(RepositoryCallback<UserProfile> callback) {
        executeCall(apiService.getProfile(), callback);
    }

    public void updateProfile(UpdateProfileRequest request, RepositoryCallback<UserProfile> callback) {
        executeCall(apiService.updateProfile(request), callback);
    }

    public void updatePassword(UpdatePasswordRequest request, RepositoryCallback<Object> callback) {
        executeCall(apiService.updatePassword(request), callback);
    }

    public void getPendingUsers(RepositoryCallback<List<PendingUser>> callback) {
        executeCall(apiService.getPendingUsers(), callback);
    }

    public void approveUser(String userId, RepositoryCallback<Object> callback) {
        executeCall(apiService.approveUser(userId), callback);
    }

    public void rejectUser(String userId, RepositoryCallback<Object> callback) {
        executeCall(apiService.rejectUser(userId), callback);
    }
}
