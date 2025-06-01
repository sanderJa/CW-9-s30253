using CW_9_s30253.Data;
using CW_9_s30253.DTOs;
using CW_9_s30253.Models;
using Microsoft.EntityFrameworkCore;

namespace CW_9_s30253.Services;

public interface IPrescriptionService
{
    Task AddPrescriptionAsync(PrescriptionRequestDto dto);
    Task<PatientDetailsDto> GetPatientDetailsAsync(int idPatient);
}


public class PrescriptionService  : IPrescriptionService
{
    private readonly AppDbContext _context;

    public PrescriptionService(AppDbContext context) 
    {
        _context = context;
    }

    public async Task AddPrescriptionAsync(PrescriptionRequestDto dto)
    {
        if (dto.Medicaments.Count > 10)
            throw new Exception("Max 10 medicaments allowed");

        if (dto.DueDate < dto.Date)
            throw new Exception("DueDate must be >= Date");

        var patient = dto.Patient.IdPatient.HasValue
            ? await _context.Patients.FindAsync(dto.Patient.IdPatient.Value)
            : null;

        if (patient == null)
        {
            patient = new Patient
            {
                FirstName = dto.Patient.FirstName,
                LastName = dto.Patient.LastName,
                BirthDate = dto.Patient.BirthDate
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        foreach (var m in dto.Medicaments)
        {
            if (!await _context.Medicaments.AnyAsync(x => x.IdMedicament == m.IdMedicament))
                throw new Exception($"Medicament with ID {m.IdMedicament} does not exist");
        }

        var prescription = new Prescription
        {
            Date = dto.Date,
            DueDate = dto.DueDate,
            IdPatient = patient.IdPatient
        };
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        foreach (var m in dto.Medicaments)
        {
            _context.PrescriptionMedicaments.Add(new PrescriptionMedicament
            {
                IdPrescription = prescription.IdPrescription,
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Description = m.Description
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<PatientDetailsDto> GetPatientDetailsAsync(int idPatient)
    {
        var patient = await _context.Patients
            .Include(p => p.Prescriptions)
            .ThenInclude(p => p.PrescriptionMedicaments)
            .ThenInclude(pm => pm.Medicament)
            .Include(p => p.Prescriptions)
            .FirstOrDefaultAsync(p => p.IdPatient == idPatient);

        if (patient == null) throw new Exception("Patient not found");

        return new PatientDetailsDto
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
            Prescriptions = patient.Prescriptions
                .OrderBy(p => p.DueDate)
                .Select(p => new PrescriptionDetailsDto
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Medicaments = p.PrescriptionMedicaments.Select(pm => new MedicamentDetailsDto
                    {
                        IdMedicament = pm.Medicament.IdMedicament,
                        Name = pm.Medicament.Name,
                        Description = pm.Medicament.Description,
                        Dose = pm.Dose
                    }).ToList()
                }).ToList()
        };
    }
}