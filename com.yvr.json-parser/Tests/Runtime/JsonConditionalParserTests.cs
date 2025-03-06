using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace YVR.JsonParser.Tests
{
    [TestFixture, Category("Json")]
    public class JsonConditionalParserTests
    {
        #region Deserialize

        [Test]
        public void Deserialize_WithoutCondition_Succeed()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            bool succeed = jsonConditionalParser.Deserialize("data", out AppDetail appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(succeed && appDetail.scover ==
                "https://apidev.yvrdream.com/vrmc/pics/eeb90489-6f91-4242-86b3-915f906a692f.png");
        }

        [Test]
        public void Deserialize_WithoutKeyWithoutCondition_SucceedOriginValueKeep()
        {
            string json = "{\"email\":\"unity@pfdn.cn\",\"active\":true}";
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(json);
            bool succeed
                = jsonConditionalParser.Deserialize("", out AccountClassWithGender accountWithGender); // Without Key

            Assert.That(succeed && accountWithGender.active);
        }

        [Test]
        public void Deserialize_WithMetCondition_Succeed()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("errCode", (int errorCode) => errorCode == 0);
            bool succeed = jsonConditionalParser.Deserialize("data", out AppDetail appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(succeed && appDetail.scover ==
                "https://apidev.yvrdream.com/vrmc/pics/eeb90489-6f91-4242-86b3-915f906a692f.png");
        }

        [Test]
        public void Deserialize_WithConditionWrongValue_ReturnFailAndNotDeserialize()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("errCode", (int errorCode) => errorCode == 1);
            bool succeed = jsonConditionalParser.Deserialize("data", out AppDetail appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed && string.IsNullOrEmpty(appDetail?.scover));
        }

        [Test]
        public void Deserialize_WithConditionWrongValWithFailCallback_CallbackBeTriggered()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("errCode", (int errorCode) => errorCode == 1);
            jsonConditionalParser.AddConditionFailCallback("errCode",
                (int? errorCode) => Debug.Log("Actual ErrorCode is 0"));
            bool succeed = jsonConditionalParser.Deserialize("data", out AppDetail _);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed);
            LogAssert.Expect(LogType.Log, "Actual ErrorCode is 0");
        }

        [Test]
        public void Deserialize_WithConditionUnExistedKey_ReturnFailAndNotDeserialize()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("unExisted", (int errorCode) => errorCode == 0);
            bool succeed = jsonConditionalParser.Deserialize("data", out AppDetail appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed && string.IsNullOrEmpty(appDetail?.scover));
        }

        [Test]
        public void Deserialize_WithConditionUnExistedKeyWithFailCallback_CallbackBeTriggered()
        {
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("unExisted", (int errorCode) => errorCode == 1);
            jsonConditionalParser.AddConditionFailCallback("unExisted",
                (int? unExited) => Debug.Log($"unExisted is null {(unExited == null).ToString()}"));
            bool succeed = jsonConditionalParser.Deserialize("data", out AppDetail _);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed);
            LogAssert.Expect(LogType.Log, "unExisted is null True");
        }

        #endregion


        [Test]
        public void Populate_WithoutCondition_SucceedOriginValueKeep()
        {
            AppDetail appDetail = new AppDetail
            {
                extraField = "ABC"
            };
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            bool succeed = jsonConditionalParser.Populate("data", appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(succeed && appDetail.scover ==
                "https://apidev.yvrdream.com/vrmc/pics/eeb90489-6f91-4242-86b3-915f906a692f.png");
            Assert.That(appDetail.extraField == "ABC");
        }

        [Test]
        public void Populate_WithoutKeyWithoutCondition_SucceedOriginValueKeep()
        {
            AccountClassWithGender accountWithGender = new AccountClassWithGender()
            {
                email = "unity@pfdn.cn",
                active = false,
                roles = new List<string>()
                {
                    "User",
                    "Admin"
                },
                gender = null
            };
            string json = "{\"email\":\"unity@pfdn.cn\",\"active\":true}";
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(json);
            bool succeed = jsonConditionalParser.Populate("", accountWithGender); // Without Key

            Assert.That(succeed && accountWithGender.active);
        }

        [Test]
        public void Populate_WithMetCondition_SucceedOriginValueKeep()
        {
            AppDetail appDetail = new AppDetail
            {
                extraField = "ABC"
            };
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("errCode", (int errorCode) => errorCode == 0);
            bool succeed = jsonConditionalParser.Populate("data", appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(succeed && appDetail.scover ==
                "https://apidev.yvrdream.com/vrmc/pics/eeb90489-6f91-4242-86b3-915f906a692f.png");
            Assert.That(appDetail.extraField == "ABC");
        }

        [Test]
        public void Populate_WithConditionWrongValue_ReturnFailAndNotDeserialize()
        {
            AppDetail appDetail = new AppDetail
            {
                extraField = "ABC"
            };
            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("errCode", (int errorCode) => errorCode == 1);
            bool succeed = jsonConditionalParser.Populate("data", appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed && string.IsNullOrEmpty(appDetail.scover));
        }

        [Test]
        public void Populate_WithConditionWrongValWithFailCallback_CallbackBeTriggered()
        {
            AppDetail appDetail = new AppDetail
            {
                extraField = "ABC"
            };

            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("errCode", (int errorCode) => errorCode == 1);
            jsonConditionalParser.AddConditionFailCallback("errCode",
                (int? errorCode) => Debug.Log("Actual ErrorCode is 0"));
            bool succeed = jsonConditionalParser.Populate("data", appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed);
            LogAssert.Expect(LogType.Log, "Actual ErrorCode is 0");
        }

        [Test]
        public void Populate_WithConditionUnExistedKey_ReturnFailAndNotDeserialize()
        {
            AppDetail appDetail = new AppDetail
            {
                extraField = "ABC"
            };

            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("unExisted", (int errorCode) => errorCode == 0);
            bool succeed = jsonConditionalParser.Populate("data", appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed && string.IsNullOrEmpty(appDetail.scover));
        }

        [Test]
        public void Populate_WithConditionUnExistedKeyWithFailCallback_CallbackBeTriggered()
        {
            AppDetail appDetail = new AppDetail
            {
                extraField = "ABC"
            };

            TextAsset jsonResource = Resources.Load<TextAsset>("appDetail");
            JsonConditionalParser jsonConditionalParser = new JsonConditionalParser(jsonResource.text);
            jsonConditionalParser.AddCondition("unExisted", (int errorCode) => errorCode == 1);
            jsonConditionalParser.AddConditionFailCallback("unExisted",
                (int? unExited) => Debug.Log($"unExisted is null {(unExited == null).ToString()}"));
            bool succeed = jsonConditionalParser.Populate("data", appDetail);
            Resources.UnloadAsset(jsonResource);

            Assert.That(!succeed);
            LogAssert.Expect(LogType.Log, "unExisted is null True");
        }
    }
}