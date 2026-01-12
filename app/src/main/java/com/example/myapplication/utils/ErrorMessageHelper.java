package com.example.myapplication.utils;

import android.content.Context;
import retrofit2.Response;

public class ErrorMessageHelper {

    public static String getErrorMessage(Context context, int httpCode, String fallbackMessage) {
        switch (httpCode) {
            case 400:
                return "Données invalides. Veuillez vérifier vos informations.";
            case 401:
                return "Session expirée. Veuillez vous reconnecter.";
            case 403:
                return "Vous n'avez pas la permission pour cette action.";
            case 404:
                return "Élément non trouvé.";
            case 500:
                return "Erreur serveur. Veuillez réessayer plus tard.";
            case 503:
                return "Service temporairement indisponible.";
            default:
                return fallbackMessage != null ? fallbackMessage : "Erreur de connexion.";
        }
    }

    public static boolean isClientError(int httpCode) {
        return httpCode >= 400 && httpCode < 500;
    }

    public static boolean isServerError(int httpCode) {
        return httpCode >= 500;
    }
}
