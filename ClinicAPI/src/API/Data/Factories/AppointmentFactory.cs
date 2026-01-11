using API.Models;

namespace API.Data.Factories;

/// <summary>
/// Factory for generating Appointment test data
/// </summary>
public class AppointmentFactory : Factory<Appointment>
{
    private static readonly string[] Reasons =
    {
        "Consultation de routine",
        "Suivi cardiaque",
        "Examen dermatologique",
        "Consultation pédiatrique",
        "Bilan de santé",
        "Douleurs abdominales",
        "Maux de tête fréquents",
        "Examen de la vue",
        "Vaccination",
        "Ordonnance pour médicaments",
        "Analyse de sang",
        "Radiographie",
        "Consultation psychiatrique",
        "Suivi post-opératoire",
        "Examen gynécologique"
    };

    private static readonly string[] DoctorNotes =
    {
        "Patient en bonne santé. Contrôle recommandé dans 6 mois.",
        "Pression artérielle stable. Continuer le traitement actuel.",
        "Éruption cutanée bénigne. Crème prescrite.",
        "Développement normal de l'enfant.",
        "Bilan complet satisfaisant. Aucun souci particulier.",
        "Symptômes probablement liés au stress. Repos recommandé.",
        "Migraines chroniques. Traitement préventif initié.",
        "Vision légèrement diminuée. Lunettes recommandées.",
        "Vaccinations à jour. Carnet mis à jour.",
        "Résultats d'analyse dans les normes.",
        "Aucune fracture détectée. Repos et anti-inflammatoires.",
        "État mental stable. Thérapie à poursuivre.",
        "Guérison normale. Retrait des sutures prévu dans 10 jours.",
        "Examen régulier. Aucune anomalie détectée.",
        "Contraception discutée. Prescription renouvelée."
    };

    public override Appointment Generate()
    {
        var status = Faker.PickRandom<AppointmentStatus>();
        var now = DateTime.UtcNow;
        DateTime appointmentDate;

        // Generate date based on status
        switch (status)
        {
            case AppointmentStatus.Completed:
                appointmentDate = Faker.Date.Past(refDate: now);
                break;
            case AppointmentStatus.Cancelled:
            case AppointmentStatus.NoShow:
                appointmentDate = Faker.Date.Between(now.AddDays(-30), now.AddDays(-1));
                break;
            default: // Scheduled
                appointmentDate = Faker.Date.Between(now, now.AddDays(30));
                break;
        }

        return new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.Empty, // Must be set explicitly
            DoctorId = Guid.Empty, // Must be set explicitly
            AppointmentDate = appointmentDate,
            DurationMinutes = Faker.PickRandom(new[] { 30, 45, 60 }),
            Status = status,
            Reason = Faker.PickRandom(Reasons),
            Notes = status == AppointmentStatus.Cancelled ? Faker.Lorem.Sentence() : null,
            DoctorNotes = status == AppointmentStatus.Completed ? Faker.PickRandom(DoctorNotes) : null,
            CreatedBy = Guid.Empty, // Must be set explicitly
            CreatedAt = Faker.Date.Past(refDate: appointmentDate),
            CancelledAt = status == AppointmentStatus.Cancelled ? appointmentDate : null
        };
    }

    /// <summary>
    /// Generate for specific patient and doctor
    /// </summary>
    public Appointment GenerateFor(Guid patientId, Guid doctorId, Guid createdBy)
    {
        return Generate(appointment =>
        {
            appointment.PatientId = patientId;
            appointment.DoctorId = doctorId;
            appointment.CreatedBy = createdBy;
        });
    }

    /// <summary>
    /// Generate scheduled appointment for future date
    /// </summary>
    public Appointment GenerateScheduled(Guid patientId, Guid doctorId, Guid createdBy, DateTime date)
    {
        var appointment = GenerateFor(patientId, doctorId, createdBy);
        appointment.AppointmentDate = date;
        appointment.Status = AppointmentStatus.Scheduled;
        appointment.CreatedAt = Faker.Date.Between(date.AddDays(-14), date.AddDays(-1));
        appointment.CancelledAt = null;
        appointment.DoctorNotes = null;
        return appointment;
    }

    /// <summary>
    /// Generate completed appointment in the past
    /// </summary>
    public Appointment GenerateCompleted(Guid patientId, Guid doctorId, Guid createdBy, DateTime date)
    {
        var appointment = GenerateFor(patientId, doctorId, createdBy);
        appointment.AppointmentDate = date;
        appointment.Status = AppointmentStatus.Completed;
        appointment.CreatedAt = Faker.Date.Between(date.AddDays(-14), date.AddDays(-1));
        appointment.DoctorNotes = Faker.PickRandom(DoctorNotes);
        appointment.CancelledAt = null;
        return appointment;
    }

    /// <summary>
    /// Generate multiple appointments for a patient
    /// </summary>
    public IEnumerable<Appointment> GenerateForPatient(Guid patientId, List<Guid> doctorIds, Guid createdBy, int count)
    {
        var now = DateTime.UtcNow;

        return Enumerable.Range(0, count).Select(i =>
        {
            var isPast = i < count / 2;
            var doctorId = Faker.PickRandom(doctorIds);

            if (isPast)
            {
                var date = Faker.Date.Between(now.AddDays(-60), now.AddDays(-1));
                return GenerateCompleted(patientId, doctorId, createdBy, date);
            }
            else
            {
                var date = Faker.Date.Between(now, now.AddDays(60));
                return GenerateScheduled(patientId, doctorId, createdBy, date);
            }
        });
    }
}
