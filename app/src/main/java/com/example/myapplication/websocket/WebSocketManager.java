package com.example.myapplication.websocket;

import android.content.Context;
import android.util.Log;
import com.example.myapplication.api.SessionManager;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import org.json.JSONObject;

import java.util.concurrent.TimeUnit;

/**
 * WebSocketManager for real-time notifications
 * Expected events: account_approved, account_rejected, new_appointment, appointment_cancelled, etc.
 */
public class WebSocketManager {

    private static final String TAG = "WebSocketManager";
    private static WebSocketManager instance;
    private OkHttpClient client;
    private WebSocket webSocket;
    private SessionManager sessionManager;
    private Context context;
    private WebSocketEventListener eventListener;
    private boolean isConnecting = false;

    public interface WebSocketEventListener {
        void onAccountApproved();
        void onAccountRejected(String reason);
        void onNewAppointment();
        void onAppointmentCancelled();
        void onAppointmentCompleted();
        void onMessageReceived(String event, JSONObject data);
        void onConnected();
        void onDisconnected();
        void onError(String error);
    }

    private WebSocketManager(Context context) {
        this.context = context.getApplicationContext();
        this.sessionManager = new SessionManager(context);

        client = new OkHttpClient.Builder()
                .readTimeout(0, TimeUnit.MILLISECONDS)
                .writeTimeout(10, TimeUnit.SECONDS)
                .connectTimeout(10, TimeUnit.SECONDS)
                .build();
    }

    public static synchronized WebSocketManager getInstance(Context context) {
        if (instance == null) {
            instance = new WebSocketManager(context);
        }
        return instance;
    }

    public void setEventListener(WebSocketEventListener listener) {
        this.eventListener = listener;
    }

    public void connect() {
        if (webSocket != null && isConnecting) {
            Log.d(TAG, "WebSocket already connecting or connected");
            return;
        }

        String token = sessionManager.getToken();
        if (token == null) {
            Log.e(TAG, "No auth token found");
            return;
        }

        isConnecting = true;

        // TODO: Update with actual WebSocket URL from backend
        // For now using placeholder - will need to configure actual server URL
        String wsUrl = "ws://26.10.1.235:8080/ws?token=" + token;

        Request request = new Request.Builder()
                .url(wsUrl)
                .build();

        webSocket = client.newWebSocket(request, new WebSocketListener() {
            @Override
            public void onOpen(WebSocket webSocket, Response response) {
                Log.d(TAG, "WebSocket connected");
                isConnecting = false;
                if (eventListener != null) {
                    eventListener.onConnected();
                }
            }

            @Override
            public void onMessage(WebSocket webSocket, String text) {
                Log.d(TAG, "Message received: " + text);
                handleMessage(text);
            }

            @Override
            public void onClosing(WebSocket webSocket, int code, String reason) {
                Log.d(TAG, "WebSocket closing: " + reason);
                isConnecting = false;
            }

            @Override
            public void onClosed(WebSocket webSocket, int code, String reason) {
                Log.d(TAG, "WebSocket closed: " + reason);
                isConnecting = false;
                if (eventListener != null) {
                    eventListener.onDisconnected();
                }
            }

            @Override
            public void onFailure(WebSocket webSocket, Throwable t, Response response) {
                Log.e(TAG, "WebSocket error", t);
                isConnecting = false;
                if (eventListener != null) {
                    eventListener.onError(t.getMessage());
                }
                // Attempt reconnect after delay
                reconnect();
            }
        });
    }

    private void handleMessage(String message) {
        try {
            JSONObject json = new JSONObject(message);
            String event = json.optString("event", "");
            JSONObject data = json.optJSONObject("data");

            if (eventListener == null) return;

            switch (event) {
                case "account_approved":
                    eventListener.onAccountApproved();
                    break;
                case "account_rejected":
                    String reason = data != null ? data.optString("reason", "") : "";
                    eventListener.onAccountRejected(reason);
                    break;
                case "new_appointment":
                    eventListener.onNewAppointment();
                    break;
                case "appointment_cancelled":
                    eventListener.onAppointmentCancelled();
                    break;
                case "appointment_completed":
                    eventListener.onAppointmentCompleted();
                    break;
                default:
                    eventListener.onMessageReceived(event, data);
                    break;
            }
        } catch (Exception e) {
            Log.e(TAG, "Error parsing message", e);
        }
    }

    private void reconnect() {
        // TODO: Implement exponential backoff reconnection logic
        // For now, simple reconnect after 5 seconds
        new android.os.Handler().postDelayed(() -> {
            if (sessionManager.isLoggedIn()) {
                Log.d(TAG, "Attempting to reconnect...");
                connect();
            }
        }, 5000);
    }

    public void disconnect() {
        if (webSocket != null) {
            webSocket.close(1000, "User logged out");
            webSocket = null;
        }
        isConnecting = false;
    }

    public void sendEvent(String event, JSONObject data) {
        if (webSocket != null) {
            try {
                JSONObject message = new JSONObject();
                message.put("event", event);
                message.put("data", data);
                webSocket.send(message.toString());
            } catch (Exception e) {
                Log.e(TAG, "Error sending message", e);
            }
        }
    }

    public boolean isConnected() {
        return webSocket != null && isConnecting;
    }
}
