using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace ProtoTraversal
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net462, baseline: true)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.NetCoreApp31)]
    public class ProtoTraversalBenchmarks
    {
        static Dictionary<string, Dictionary<string, Dictionary<string, string>>> translations = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        public ProtoTraversalBenchmarks()
        {

        }

       	public static IMessage TranslateProtoMessage(IMessage message, string language)
		{
			var fields = message.Descriptor.Fields.InFieldNumberOrder();
			var languageDictionary = translations[language];
			foreach (var field in fields)
			{

				if (field.FieldType == FieldType.String)
				{
					field.CustomOptions.TryGetBool(GreetingExtensions.ShouldTranslate.FieldNumber, out bool shouldTranslate);
					if (shouldTranslate)
					{
						field.CustomOptions.TryGetString(GreetingExtensions.TranslationKey.FieldNumber, out string translationKey);
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
			}
			return message;
		}

        [Benchmark]
        [ArgumentsSource(nameof(GetSummary))]
        public void SingleArgument(VersionSummary versionSummary){
            
            TranslateProtoMessage(versionSummary,"hi");
            
            //translations.ContainsKey("hi");
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetRequest))]
        public void SingleField(HelloRequest helloRequest){
            TranslateProtoMessage(helloRequest,"hi");
        }
        public IEnumerable<object> GetSummary(){
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
				Hello = new HelloRequest{
						Fullname = "one plus",
						Name = "one"
					}
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
            yield return  versionSummary ;
        }

        public IEnumerable<object> GetRequest(){
            var	request = new HelloRequest{
						Fullname = "one plus",
						Name = "one"
					};
			
            yield return  request ;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.OutputEncoding = Encoding.UTF8;
            translations["hi"] = new Dictionary<string, Dictionary<string, string>>();
			translations["hi"]["make_name"] = new Dictionary<string, string>();
			translations["hi"]["model_name"] = new Dictionary<string, string>();
			translations["hi"]["version_name"] = new Dictionary<string, string>();
			translations["hi"]["names"] = new Dictionary<string, string>();
			translations["hi"]["make_name"]["Tata"] = "Tata";
			translations["hi"]["model_name"]["Altroz"] = "Altroz";
			translations["hi"]["version_name"]["XZ Petrol"] = "XZ Petrol";
			translations["hi"]["version_name"]["XT Petrol"] = "XT Petrol";
			translations["hi"]["version_name"]["XE Diesel"] = "XE Diesel";
			translations["hi"]["names"]["one"] = "one";
			translations["hi"]["names"]["two"] = "two";
        }
    }
}
