using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using YVR.Utilities;

namespace YVR.JsonParser.Tests
{
    [TestFixture, Category("Json")]
    public class JsonParserTests
    {
        #region TestData

        private const string k_TestJsonFolder = "Packages/com.yvr.json-parser/Tests/Runtime/JsonStr";

        public static object[] jsonStrings2 =
        {
            new object[]
            {
                "1,2,3,4,5", // Invalid
                false
            },
            new object[]
            {
                "[1,2,3,4,5]",
                true
            },
            new object[]
            {
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}",
                true
            },
            new object[]
            {
                "{\"email\"\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}", // Lack of colon
                false
            }
        };

        public static object[] normalObjectAndJsonList =
        {
            new object[]
            {
                new AccountClass()
                {
                    email = "unity@pfdn.cn",
                    active = true,
                    roles = new List<string>()
                    {
                        "User",
                        "Admin"
                    }
                },
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}"
            },
            new object[]
            {
                new List<int>() {1, 2, 3, 4, 5},
                "[1,2,3,4,5]"
            },
            new object[]
            {
                1,
                "1"
            },
            new object[]
            {
                new AccountClassWithSerializedPrivate(),
                "{}"
            }
        };

        public static object[] diffNameObjectAndJsonList =
        {
            new object[]
            {
                new AccountClassWithDiffName()
                {
                    emailDiff = "unity@pfdn.cn",
                    activeDiff = true,
                    rolesDiff = new List<string>()
                    {
                        "User",
                        "Admin"
                    }
                },
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}"
            },
            new object[]
            {
                new AccountDerivedClassWithDiffName()
                {
                    emailDiff = "unity@pfdn.cn",
                    activeDiff = true,
                    rolesDiff = new List<string>()
                    {
                        "User",
                        "Admin"
                    }
                },
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}"
            }
        };

        public static object[] missingFieldAndJsonList =
        {
            new object[]
            {
                new AccountClassWithoutRoles()
                {
                    email = "unity@pfdn.cn",
                    active = true
                },
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}"
            }
        };

        public static object[] extraFieldAndJsonList =
        {
            new object[]
            {
                new AccountClassWithGender()
                {
                    email = "unity@pfdn.cn",
                    active = true,
                    roles = new List<string>()
                    {
                        "User",
                        "Admin"
                    },
                    gender = null
                },
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}"
            }
        };


        public static object[] listObjectAndJsonList =
        {
            new object[]
            {
                new AccountClass()
                {
                    email = "unity@pfdn.cn",
                    active = true,
                    roles = new List<string>()
                    {
                        "User",
                        "Admin"
                    }
                }, // Origin Object
                "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"User\",\"Admin\"]}", // Json
                "roles", // Wrap
                new List<string>() {"User", "Admin"}, // Target List
                typeof(string) // Element type
            },
            new object[]
            {
                new List<int>() {1, 2, 3, 4, 5},
                "[1,2,3,4,5]",
                "",
                new List<int>() {1, 2, 3, 4, 5},
                typeof(int)
            }
        };

        #endregion

        #region DeserializeTests

        [Test, TestCaseSource(nameof(normalObjectAndJsonList))]
        public void DeserializeObject_JsonForClass_GetCorrectObject(object targetObject, string originJson)
        {
            object deserializedObject = JsonParserMgr.DeserializeObject(originJson, targetObject.GetType());
            Assert.That(deserializedObject.ReflectEquals(targetObject));
        }

        [Test, TestCaseSource(nameof(normalObjectAndJsonList))]
        public void DeserializeObjectGeneric_JsonForClass_GetCorrectObject(object targetObject, string originJson)
        {
            MethodInfo methodInfo = typeof(JsonParserMgr).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                         .Single(method => method.Name == "DeserializeObject" &&
                                                                           method.IsGenericMethod)
                                                         .MakeGenericMethod(targetObject.GetType());

            object deserializedObject = methodInfo.Invoke(null, new object[] {originJson});
            Assert.That(deserializedObject.ReflectEquals(targetObject));
        }

        [Test, TestCaseSource(nameof(diffNameObjectAndJsonList))]
        public void DeserializeObject_DifferentName_GetCorrectObject(object targetObject, string originJson)
        {
            object deserializedObject = JsonParserMgr.DeserializeObject(originJson, targetObject.GetType());
            Assert.That(deserializedObject.ReflectEquals(targetObject));
        }

        [Test, TestCaseSource(nameof(missingFieldAndJsonList))]
        public void DeserializeObject_MissField_GetCorrectObject(object targetObject, string originJson)
        {
            object deserializedObject = JsonParserMgr.DeserializeObject(originJson, targetObject.GetType());
            Assert.That(deserializedObject.ReflectEquals(targetObject));
        }

        [Test, TestCaseSource(nameof(extraFieldAndJsonList))]
        public void DeserializeObject_ExtraField_GetCorrectObject(object targetObject, string originJson)
        {
            object deserializedObject = JsonParserMgr.DeserializeObject(originJson, targetObject.GetType());
            Assert.That(deserializedObject.ReflectEquals(targetObject));
        }

        [Test]
        public void TryDeserializeObject_ExistedNameCorrectTypeIntValue_CorrectValue()
        {
            var jsonResource = Resources.Load<TextAsset>("appDetail");
            bool succeed = JsonParserMgr.TryDeserializeObject(jsonResource.text, "errCode", out int errorCode);
            Resources.UnloadAsset(jsonResource);

            Assert.That(succeed && errorCode == 0);
        }

        [Test]
        public void TryDeserializeObject_ExistedNameCorrectTypeStringValue_CorrectValue()
        {
            string jsonPath = Path.GetFullPath($"{k_TestJsonFolder}/LocalAppData.json");
            string jsonString = File.ReadAllText(jsonPath);
            bool succeed = JsonParserMgr.TryDeserializeObject(jsonString, "versionName", out string version);

            Assert.That(succeed && version.Equals("16.0.27"));
        }

        [Test]
        public void TryDeserializeObject_ExistedNameCorrectTypeBoolValue_CorrectValue()
        {
            string jsonPath = Path.GetFullPath($"{k_TestJsonFolder}/ScreenRecordBroadcast.json");
            string jsonString = File.ReadAllText(jsonPath);
            bool succeed = JsonParserMgr.TryDeserializeObject(jsonString, "start", out bool version);
            
            Assert.That(succeed, "Is not succeed");
            Assert.That(version, "Get wrong bool value");
        }


        [Test]
        public void TryDeserializeObject_ExistedNameCorrectTypeComplicatedValue_CorrectValue()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            bool succeed = JsonParserMgr.TryDeserializeObject(jsonResource.text, "data", out AppDetail appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(succeed && appDetail.scover ==
                        "https://apidev.yvrdream.com/vrmc/pics/eeb90489-6f91-4242-86b3-915f906a692f.png");
        }

        [Test]
        public void TryDeserializeObject_UnExistedTargetNameCorrectType_ReturnFalse()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            bool succeed = JsonParserMgr.TryDeserializeObject(jsonResource.text, "UnExist", out int _);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed);
        }

        #endregion

        #region PopulateTests

        [Test]
        public void PopulateObject_ExistedJsonFieldField_JsonFieldValueReplace()
        {
            AccountClassWithGender accountForPopulate = new AccountClassWithGender()
            {
                active = false,
            };

            string jsonForPopulate = "{\"active\":true}";

            JsonParserMgr.PopulateObject(jsonForPopulate, accountForPopulate);

            Assert.That(accountForPopulate.active);
        }

        [Test]
        public void PopulateObject_ExtraClassField_ExtraClassFieldValueKeep()
        {
            AccountClassWithGender accountForPopulate = new AccountClassWithGender()
            {
                active = true,
                gender = "Female"
            };

            string jsonForPopulate = "{\"active\":true}";

            JsonParserMgr.PopulateObject(jsonForPopulate, accountForPopulate);

            Assert.That(accountForPopulate.gender == "Female");
        }

        [Test]
        public void PopulateObject_ExistedListWithDefault_JsonListElementAdded()
        {
            AccountClassWithGender accountForPopulate = new AccountClassWithGender()
            {
                email = "unity@pfdn.cn",
                active = true,
                roles = new List<string>()
                {
                    "User",
                    "Admin"
                },
                gender = null
            };

            string jsonForPopulate = "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"Ben\",\"Tony\"]}";
            JsonParserMgr.PopulateObject(jsonForPopulate, accountForPopulate);

            Assert.That(accountForPopulate.roles.Count == 4);
        }

        [Test]
        public void PopulateObject_ExistedListWithNotReuse_JsonListElementReplaced()
        {
            AccountClassWithGender accountForPopulate = new AccountClassWithGender()
            {
                email = "unity@pfdn.cn",
                active = true,
                roles = new List<string>()
                {
                    "User",
                    "Admin"
                },
                gender = null
            };

            string jsonForPopulate = "{\"email\":\"unity@pfdn.cn\",\"active\":true,\"roles\":[\"Ben\",\"Tony\"]}";
            JsonParserMgr.PopulateObject(jsonForPopulate, accountForPopulate, false);


            Assert.That(accountForPopulate.roles.Count == 2 && accountForPopulate.roles[0] == "Ben" &&
                        accountForPopulate.roles[1] == "Tony");
        }

        [Test]
        public void TryPopulateObject_ExistedNameCorrectTypeComplicatedValue_CorrectValue()
        {
            AppDetail appDetail = new AppDetail();

            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");

            bool succeed = JsonParserMgr.TryPopulateObject(jsonResource.text, "data", appDetail);

            Resources.UnloadAsset(jsonResource);
            Assert.That(appDetail.extraField == "ArbitraryValue"); // Origin Value keep
            Assert.That(succeed && appDetail.scover ==
                        "https://apidev.yvrdream.com/vrmc/pics/eeb90489-6f91-4242-86b3-915f906a692f.png");
        }

        #endregion

        [Test, TestCaseSource(nameof(jsonStrings2))]
        public void IsValidJson(string jsonStr, bool result)
        {
            Assert.That(JsonParserMgr.IsValidJson(jsonStr) == result);
        }

        [Test, TestCaseSource(nameof(normalObjectAndJsonList))]
        public void SerializeObject_ClassWithList_GetCorrectJson(object originObject, string targetJson)
        {
            string serializeJson = JsonParserMgr.SerializeObject(originObject);
            Assert.That(serializeJson, Is.EqualTo(targetJson));
        }

        [Test, TestCaseSource(nameof(diffNameObjectAndJsonList))]
        public void SerializeObject_ClassWithDifferentName_GetCorrectJson(object originObject, string targetJson)
        {
            string serializeJson = JsonParserMgr.SerializeObject(originObject);
            Assert.That(serializeJson, Is.EqualTo(targetJson));
        }


        [Test, TestCaseSource(nameof(listObjectAndJsonList))]
        public void GetList_ListInObject_GetCorrectObject(object originObject, string json, string wrapper,
                                                          object targetList, Type targetElementType)
        {
            MethodInfo methodInfo = typeof(JsonParserMgr).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                         .Single(method => method.Name == "GetList" &&
                                                                           method.IsGenericMethod)
                                                         .MakeGenericMethod(targetElementType);
            object result = methodInfo.Invoke(null, new object[] {json, wrapper});

            Assert.That(result.ReflectEquals(targetList));
        }

        [Test]
        public void GetList_ComplexElement_GetCorrectObject()
        {
            string jsonPath = Path.GetFullPath($"{k_TestJsonFolder}/AppDownloadInfo.json");
            string jsonString = File.ReadAllText(jsonPath);
            List<DownloadInfo> result = JsonParserMgr.GetList<DownloadInfo>(jsonString);
            var target = new List<DownloadInfo>()
            {
                new()
                {
                    isSystem = false,
                    fileSize = 704264327,
                    downloadLength = 52958191,
                    appId = 3308841940,
                    appName = "Punch",
                    packageName = "com.hemudu.punch",
                    initStatus = 3
                },
                new()
                {
                    isSystem = false,
                    fileSize = 107750076,
                    downloadLength = 40807195,
                    appId = 3823345597,
                    appName = "爱奇艺 HD",
                    packageName = "com.qiyi.video.pad",
                    initStatus = 3
                },
                new()
                {
                    isSystem = false,
                    fileSize = 271464018,
                    downloadLength = 0,
                    appId = 9268865956,
                    appName = "暴走篮球",
                    packageName = "io.realcast.justhoops.yvr",
                    initStatus = 7
                },
            };

            Assert.That(result.SequenceEqual(target));
        }
    }
}