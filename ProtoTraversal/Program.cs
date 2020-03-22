using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace ProtoTraversal
{
	class Program
	{
		static Dictionary<string, Dictionary<string, Dictionary<string, string>>> translations = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

		static void Main(string[] args)
		{
            Console.OutputEncoding = Encoding.UTF8;
			translations["hi"] = new Dictionary<string, Dictionary<string, string>>();
			translations["hi"]["make_name"] = new Dictionary<string, string>();
			translations["hi"]["model_name"] = new Dictionary<string, string>();
			translations["hi"]["version_name"] = new Dictionary<string, string>();
			translations["hi"]["names"] = new Dictionary<string, string>();
			translations["hi"]["make_name"]["Tata"] = "टाटा‎‌";
			translations["hi"]["model_name"]["Altroz"] = "अल्ट्रोज़‎‌";
			translations["hi"]["version_name"]["XZ Petrol"] = "एक्सजेड पेट्रोल‎‌";
			translations["hi"]["version_name"]["XT Petrol"] = "एक्सटी पेट्रोल‎‌‎‌";
			translations["hi"]["version_name"]["XE Diesel"] = "एक्सई डीज़ल‎‌‎‌";
			translations["hi"]["names"]["one"] = "Ek";
			translations["hi"]["names"]["two"] = "Do";
			Console.WriteLine("Hello World!");
			VersionSummary versionSummary = new VersionSummary
			{
				ApplicationId = 1,
				Id = 4228,
				ImagePath = "https://imgd.aeplcdn.com/grey.gif",
				MakeId = 10,
				MakeMaskingName = "tata",
				MakeName = "Tata",
				MaskingName = "xzpetrol",
				Name = "XZ Petrol",
				ModelId = 5068,
				ModelMaskingName = "altroz",
				ModelName = "Altroz",
				Status = MmvStatus.New,
				UpdatedOn = "some date",
			};
			versionSummary.HelloRequests.AddRange(new Google.Protobuf.Collections.RepeatedField<HelloRequest>{
					new HelloRequest{
						Fullname = "one plus",
						Name = "one"
					},
					new HelloRequest{
						Fullname = "two plus",
						Name = "two"
					}
				});
			versionSummary.RandomNumbers.AddRange(new List<int> { 1, 2, 3, 4, 5 });
			versionSummary.SimilarVersionNames.AddRange(new List<string> { "XT Petrol", "XE Diesel" });
			//HelloReply hello = new HelloReply
			//HelloRequest.Descriptor.Fields.InFieldNumberOrder()
			var fullnameDescriptor = HelloRequest.Descriptor.FindFieldByName("name");
			//var optionValueb = fullnameDescriptor.GetOption(GreetingExtensions.ShouldTranslate);
			fullnameDescriptor.CustomOptions.TryGetBool(GreetingExtensions.ShouldTranslate.FieldNumber, out bool optionValue);
			Console.WriteLine(optionValue);
			fullnameDescriptor.CustomOptions.TryGetString(GreetingExtensions.TranslationKey.FieldNumber, out string optionValueb);
			Console.WriteLine(optionValueb);
			Console.WriteLine(versionSummary.GetType().FullName);
			TranslateProtoMessage(versionSummary, "hi");
			Console.WriteLine(versionSummary.ToString());
            Console.WriteLine(translations["hi"]["make_name"]["Tata"]);
		}

		public static IMessage TranslateProtoMessage(IMessage message, string language)
		{
			var fields = message.Descriptor.Fields.InFieldNumberOrder();
			var languageDictionary = translations[language];
			foreach (var field in fields)
			{

				Console.WriteLine(field.FieldType);
				Console.WriteLine(field.FullName);
				if (field.FieldType == FieldType.String)
				{
					field.CustomOptions.TryGetBool(GreetingExtensions.ShouldTranslate.FieldNumber, out bool shouldTranslate);
					if (shouldTranslate)
					{
						field.CustomOptions.TryGetString(GreetingExtensions.TranslationKey.FieldNumber, out string translationKey);
						Console.WriteLine(translationKey);
						if (field.IsRepeated)
						{
							var items = (IList<string>)field.Accessor.GetValue(message);
							for (var i = 0; i < items.Count; i++)
							{
								// TODO: check contains key, replace with translate function
								items[i] = languageDictionary[translationKey][items[i]];
							}
						}
						else
						{
							var oldString = (string)field.Accessor.GetValue(message);
                            // TODO: check contains key, replace with translate function
							var newString = languageDictionary[translationKey][oldString];
							field.Accessor.SetValue(message, newString);
						}
					}

				}
				else if (field.FieldType == FieldType.Message)
				{
					if (field.IsRepeated)
					{
						var nestedMessageList = (IList)field.Accessor.GetValue(message);
						for (var i = 0; i < nestedMessageList.Count; i++)
						{
							TranslateProtoMessage((IMessage)nestedMessageList[i], language);
						}
					}
					else
					{
						var nestedMessage = (IMessage)field.Accessor.GetValue(message);
						field.Accessor.SetValue(message, TranslateProtoMessage(nestedMessage, language));
					}
				}

				Console.WriteLine("-----------------");

			}
			return message;
		}

	}
}
