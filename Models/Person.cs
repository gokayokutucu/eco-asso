using System;
namespace EcoAsso.Models {
    public class Person {
	public int PersonID { get; set; }
	public string Email { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string PhoneNumber { get; set; }
	public bool IsMember { get; set; }
	public bool IsSubscriber { get; set; }
    }
}
