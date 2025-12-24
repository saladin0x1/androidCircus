package com.example.myapplication.api.models;

public class LoginResponse {
    private boolean success;
    private String error;
    private Data data;

    public boolean isSuccess() { return success; }
    public String getError() { return error; }
    public Data getData() { return data; }

    public static class Data {
        private String userId;
        private String firstName;
        private String lastName;
        private String email;
        private String role;
        private String token;
        private String roleSpecificId;

        public String getUserId() { return userId; }
        public String getFirstName() { return firstName; }
        public String getLastName() { return lastName; }
        public String getEmail() { return email; }
        public String getRole() { return role; }
        public String getToken() { return token; }
        public String getRoleSpecificId() { return roleSpecificId; }
    }
}
