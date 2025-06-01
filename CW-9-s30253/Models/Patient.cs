using System.ComponentModel.DataAnnotations;

namespace CW_9_s30253.Models;

public class Patient
{
    [Key]
    public int IdPatient { get; set; }
    [MaxLength(100)]
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; }= null!;
}