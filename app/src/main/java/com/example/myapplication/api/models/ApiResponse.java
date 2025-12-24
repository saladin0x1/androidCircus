package com.example.myapplication.api.models;

public class ApiResponse<T> {
    private boolean success;
    private T data;
    private Error error;

    public boolean isSuccess() { return success; }
    public T getData() { return data; }
    public Error getError() { return error; }

    public static class Error {
        private String code;
        private String message;

        public String getCode() { return code; }
        public String getMessage() { return message; }
    }
}
