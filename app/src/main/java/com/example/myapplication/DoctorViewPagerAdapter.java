package com.example.myapplication;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentActivity;
import androidx.viewpager2.adapter.FragmentStateAdapter;
import com.example.myapplication.fragments.AgendaFragment;
import com.example.myapplication.fragments.PatientDossierFragment;
import com.example.myapplication.fragments.PatientsFragment;

public class DoctorViewPagerAdapter extends FragmentStateAdapter {

    public DoctorViewPagerAdapter(@NonNull FragmentActivity fragmentActivity) {
        super(fragmentActivity);
    }

    @NonNull
    @Override
    public Fragment createFragment(int position) {
        switch (position) {
            case 0:
                return new AgendaFragment();
            case 1:
                return new PatientsFragment();
            case 2:
                return new PatientDossierFragment();
            default:
                return new AgendaFragment();
        }
    }

    @Override
    public int getItemCount() {
        return 3;
    }
}
