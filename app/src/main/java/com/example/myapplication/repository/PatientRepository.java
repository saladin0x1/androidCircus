package com.example.myapplication.repository;

import android.content.Context;
import com.example.myapplication.api.models.MedicalHistoryDTO;
import com.example.myapplication.api.models.PatientDTO;
import com.example.myapplication.api.models.UpdatePatientNotesRequest;
import java.util.List;

public class PatientRepository extends Repository {

    private static PatientRepository instance;

    private PatientRepository(Context context) {
        super(context);
    }

    public static synchronized PatientRepository getInstance(Context context) {
        if (instance == null) {
            instance = new PatientRepository(context);
        }
        return instance;
    }

    public void getPatients(String search, RepositoryCallback<List<PatientDTO>> callback) {
        executeCall(apiService.getPatients(search), callback);
    }

    public void getPatient(String id, RepositoryCallback<PatientDTO> callback) {
        executeCall(apiService.getPatient(id), callback);
    }

    public void getPatientNotes(String id, RepositoryCallback<String> callback) {
        executeCall(apiService.getPatientNotes(id), callback);
    }

    public void updatePatientNotes(String id, UpdatePatientNotesRequest request, RepositoryCallback<String> callback) {
        executeCall(apiService.updatePatientNotes(id, request), callback);
    }

    public void getMedicalHistory(String id, RepositoryCallback<MedicalHistoryDTO> callback) {
        executeCall(apiService.getPatientMedicalHistory(id), callback);
    }
}
