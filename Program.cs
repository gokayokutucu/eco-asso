using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EcoAsso.Context;
using EcoAsso.Models;
using Terminal.Gui;

class EcologyAssociation {
	static string directory;

	static FrameView ShowOtherStuff () {
		var firstName = new Label (1, 1, "Name: ");
		var firstNameText = new TextField (7, 1, 20, "") { Id = "firstName" };

		var lastName = new Label (30, 1, "Surname: ");
		var lastNameText = new TextField (39, 1, 20, "") { Id = "lastName", CanFocus = true };

		var phoneNumber = new Label (1, 3, "Phone: ");
		var phoneNumberText = new TextField (7, 3, 20, "") { Id = "phoneNumber", CanFocus = true };

		var frame = new FrameView (new Rect (3, 3, 63, 7), "*") {
			firstName,
			firstNameText,
			lastName,
			lastNameText,
			phoneNumber,
			phoneNumberText
		};
		firstNameText.EnsureFocus ();
		return frame;
	}

	static FrameView ShowEntries (View container) {
		var email = new Label ("E-mail: ") { X = 3, Y = 1 };
		var emailText = new TextField ("") {
			X = Pos.Right (email),
				Y = Pos.Top (email),
				Width = 53,
				Id = "email"
		};

		var frame = ShowOtherStuff ();

		var checkBoxNews = new FrameView (new Rect (3, 14, 50, 3), "Notification") {
			new CheckBox (1, 0, "I want to receive newsletters", true) { Id = "newsletter" }
		};

		var radioGroup = new RadioGroup (1, 0, new [] { "I want to become a participant", "Join the Food Community." }, 0) {
			Id = "recordType",
				SelectionChanged = (choose) => {
					if (choose == 1) {
						container.Add (frame);
						container.SetFocus (frame);
					} else
						container.Remove (frame);
				}
		};

		// Add some content
		container.Add (
			email,
			emailText,
			new FrameView (new Rect (3, 10, 50, 4), "Record Types") {
				radioGroup,
			},
			checkBoxNews,
			new Button ("Save") { X = 3, Y = 18, Clicked = () => SaveEvent (container, frame) },
			new Button ("Cancel") { X = 14, Y = 18, Clicked = () => NewMember (container, frame) },
			new Label ("Press F9 (on Unix ESC+9 is an alias) to activate the menubar") { X = 3, Y = 20 }
		);

		return frame;
	}

	static void NewMember (View container, FrameView frame) {
		var d = new Dialog (
			"New Member", 40, 6,
			new Button ("Ok", is_default : true) {
				Clicked = () => {
					ClearForm (container, frame);
					Application.RequestStop ();
				}
			},
			new Button ("Cancel") { Clicked = () => { Application.RequestStop (); } });
		Application.Run (d);
	}

	//How to get ALL child controls of a Windows Forms form of a specific type (Button/Textbox)? -> https://stackoverflow.com/a/3426721/4550745
	static IEnumerable<View> GetAll (View view, Type type) {
		var views = view.Subviews.Cast<View> ();

		return views.SelectMany (v => GetAll (v, type))
			.Concat (views)
			.Where (c => c.GetType () == type);
	}

	static List<T> GetAllViews<T> (View container) where T : class {
		return GetAll (container, typeof (T))
			.Cast<T> ()
			.ToList ();
	}

	static void SaveEvent (View container, FrameView frame) {
		//Validate
		var textFieldList = GetAllViews<TextField> (container);
		var textFields = textFieldList.ToDictionary (key => key.Id, value => value.Text);
		var recordType = GetAllViews<RadioGroup> (container)
			.FirstOrDefault (r => r.Id == "recordType");
		var newsletter = GetAllViews<CheckBox> (container)
			.FirstOrDefault (r => r.Id == "newsletter");

		if (Validate (textFields)) {
			var result = Save (new Person () {
				Email = textFields["email"].ToString (),
					FirstName = textFields.Count > 1 ? textFields["firstName"]?.ToString () : string.Empty,
					LastName = textFields.Count > 1 ? textFields["lastName"]?.ToString () : string.Empty,
					PhoneNumber = textFields.Count > 1 ? textFields["phoneNumber"]?.ToString () : string.Empty,
					IsMember = recordType != null && Convert.ToBoolean (recordType.Selected),
					IsSubscriber = newsletter != null && newsletter.Checked
			});

			if (result) {
				var n = MessageBox.Query (50, 5, "Info", "Successfully saved", "Ok");
				if (n == 0)
					ClearForm (container, frame);
			}
		}
	}

	static bool Save (Person person) {
		try {
			using (var db = new YeryuzuContext (directory)) {
				if (person == null) return false;
				db.People.Add (person);
				return db.SaveChanges () > 0;
			}
		} catch (Exception ex) {
			MessageBox.ErrorQuery (50, 5, "Error", ex.Message, "Ok");
			return false;
		}

	}

	static bool Validate (Dictionary<NStack.ustring, NStack.ustring> textFields) {
		Regex regexEmail = new Regex (@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
		Regex regexName = new Regex (@"^[A-Za-z ,.'-]+$");
		Regex regexPhone = new Regex (@"^\(?0?([0-9]{3})\)?([ .-]?)([0-9]{3})\2?([0-9]{2})\2?([0-9]{2})$");

		var email = textFields["email"].ToString ();
		if (email.Length < 1 || !regexEmail.Match (email).Success) {
			MessageBox.ErrorQuery (50, 5, "Error", "Please enter a valid e-mail address", "Ok");
			return false;
		}
		if (textFields.Count > 1) {
			var firstName = textFields["firstName"].ToString ();
			var lastName = textFields["lastName"].ToString ();
			var phoneNumber = textFields["phoneNumber"].ToString ();
			if (firstName.Length < 1 || lastName.Length < 1 || phoneNumber.Length < 1) {
				MessageBox.ErrorQuery (50, 5, "Error", "You can't pass this field as empty", "Ok");
				return false;
			}

			if (!regexName.Match (firstName).Success || !regexName.Match (lastName).Success) {
				MessageBox.ErrorQuery (50, 5, "Error", "Invalid character in your name or lastname", "Ok");
				return false;
			}

			if (!regexPhone.Match (phoneNumber).Success) {
				MessageBox.ErrorQuery (50, 5, "Error", "Invalid phone number", "Ok");
				return false;
			}
			return true;
		}
		return true;
	}

	static void ClearForm (View container, FrameView frame) {
		GetAllViews<TextField> (container)
			.ForEach ((TextField textField) => textField.Text = string.Empty);
		var recordType = GetAllViews<RadioGroup> (container)
			.FirstOrDefault (r => r.Id == "recordType");
		recordType.Selected = 0;

		var newsletter = GetAllViews<CheckBox> (container)
			.FirstOrDefault (r => r.Id == "newsletter");
		newsletter.Checked = true;
		container.Remove (frame);
	}

	static bool Quit () {
		var n = MessageBox.Query (50, 7, "Quit", "Are you sure you want to quit awesome app?", "Yes", "No");
		return n == 0;
	}

	static void Close () {
		MessageBox.ErrorQuery (50, 5, "Error", "There is nothing to close", "Ok");
	}

	static void Main (string[] args) {
		// /Users/gokayokutucu/Downloads/gui.cs-master/StandaloneExample/
		directory = args.Length > 0 ? args[0] : string.Empty;
		//Application.UseSystemConsole = true;
		Application.Init ();

		var top = Application.Top;
		var tframe = top.Frame;

		var win = new Window ("Yeryuzu Association Record Book") {
			X = 0,
				Y = 1,
				Width = Dim.Fill (),
				Height = Dim.Fill () - 1
		};

		var frame = ShowEntries (win);

		var menu = new MenuBar (new MenuBarItem[] {
			new MenuBarItem ("_File", new MenuItem[] {
					new MenuItem ("_New", "New member", () => NewMember (win, frame)),
						//new MenuItem ("_Open", "", null),
						new MenuItem ("_Close", "", () => Close ()),
						new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
				}),
				new MenuBarItem ("_Edit", new MenuItem[] {
					new MenuItem ("_Copy", "", null),
						new MenuItem ("C_ut", "", null),
						new MenuItem ("_Paste", "", null)
				})
		});

		top.Add (win, menu);
		top.Add (menu);
		Application.Run ();
	}
}