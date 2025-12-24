package com.example.myapplication.api;

import android.content.Context;
import android.content.SharedPreferences;

public class SessionManager {
    private static final String PREF_NAME = "MedicalCabinetPrefs";
    private static final String KEY_TOKEN = "token";
    private static final String KEY_USER_ID = "userId";
    private static final String KEY_ROLE = "role";
    private static final String KEY_NAME = "name";
    private static final String KEY_EMAIL = "email";
    private static final String KEY_ROLE_SPECIFIC_ID = "roleSpecificId";
    private static final String KEY_IS_LOGGED_IN = "isLoggedIn";

    private SharedPreferences prefs;
    private SharedPreferences.Editor editor;

    public SessionManager(Context context) {
        prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        editor = prefs.edit();
    }

    public void saveSession(String token, String userId, String role, String name, String email, String roleSpecificId) {
        editor.putBoolean(KEY_IS_LOGGED_IN, true);
        editor.putString(KEY_TOKEN, token);
        editor.putString(KEY_USER_ID, userId);
        editor.putString(KEY_ROLE, role);
        editor.putString(KEY_NAME, name);
        editor.putString(KEY_EMAIL, email);
        editor.putString(KEY_ROLE_SPECIFIC_ID, roleSpecificId);
        editor.apply();
    }

    public String getToken() {
        return prefs.getString(KEY_TOKEN, null);
    }

    public String getAuthHeader() {
        String token = getToken();
        return token != null ? "Bearer " + token : null;
    }

    public boolean isLoggedIn() {
        return prefs.getBoolean(KEY_IS_LOGGED_IN, false);
    }

    public String getUserName() {
        return prefs.getString(KEY_NAME, "");
    }
    
    public String getUserEmail() {
        return prefs.getString(KEY_EMAIL, "");
    }

    public String getUserRole() {
        return prefs.getString(KEY_ROLE, "");
    }

    public String getRoleSpecificId() {
        return prefs.getString(KEY_ROLE_SPECIFIC_ID, "");
    }

    public void logout() {
        editor.clear();
        editor.apply();
    }
}
