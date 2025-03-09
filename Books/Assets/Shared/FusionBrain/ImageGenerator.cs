using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.FusionBrain
{
    public class ImageGenerator
    {
        public struct ModelsResponse
        {
            public int id;
            public string name;
            public float version;
            public string type;
        }

        public struct ImageParams
        {
            public struct Params
            {
                public string query;
            }

            public string type;
            public int numImages;
            public int width;
            public int height;
            public string negativePromptUnclip;
            public Params generateParams;
        }

        public struct ImageStatus
        {
            public enum Status
            {
                UNDEFINED,
                INITIAL,
                PROCESSING,
                DONE,
                FAIL,
            }

            public string uuid;
            public Status status;
        }

        public struct ImageResult
        {
            public string uuid;
            public ImageStatus.Status status;
            public List<string> images;
            public string errorDescription;
            public bool censored;
        }

        public async UniTask<Texture2D> GetImage(string promt, string negativePromt = null)
        {
            //https://fusionbrain.ai/docs/doc/poshagovaya-instrukciya-po-upravleniu-api-kluchami/
            var api_key = "FC73B4397CB2D011867DA5289469B7D5";
            var secret_key = "581C0D9B4306439CC5AEA8163145979D";

            var mainURL = "https://api-key.fusionbrain.ai/key";

            //authorize
            Debug.Log($"authorize start");
            var url = $"{mainURL}/api/v1/models";
            var modelId = -1;
            using (var authRequest = UnityWebRequest.Get(url))
            {
                authRequest.SetRequestHeader("X-Key", $"Key {api_key}");
                authRequest.SetRequestHeader("X-Secret", $"Secret {secret_key}");

                await authRequest.SendWebRequest();

                var data = JsonConvert.DeserializeObject<List<ModelsResponse>>(authRequest.downloadHandler.text);
                modelId = data[0].id;
            }
            Debug.Log($"authorize done");

            if (string.IsNullOrEmpty(negativePromt))
                negativePromt = "яркие цвета, кислотность, высокая контрастность";

            var postData = new ImageParams
            {
                type = "GENERATE",
                width = 512,
                height = 512,
                numImages = 1,
                negativePromptUnclip = negativePromt,
                generateParams = new ImageParams.Params
                {
                    query = promt,
                },
            };

            url = $"{mainURL}/api/v1/text2image/run";
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("model_id", modelId.ToString(), Encoding.UTF8, "application/json"),
                new MultipartFormDataSection("params", JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json")
            };

            //run generate
            Debug.Log($"run generate start");
            ImageStatus imageStatus;
            using (var request = UnityWebRequest.Post(url, form))
            {
                request.SetRequestHeader("X-Key", $"Key {api_key}");
                request.SetRequestHeader("X-Secret", $"Secret {secret_key}");

                await request.SendWebRequest();

                imageStatus = JsonConvert.DeserializeObject<ImageStatus>(request.downloadHandler.text);
            }
            Debug.Log($"run generate done");

            //poling
            url = $"{mainURL}/api/v1/text2image/status/{imageStatus.uuid}";
            var imageCurrentStatus = imageStatus.status;
            var images = new List<string>();
            while(imageCurrentStatus != ImageStatus.Status.DONE)
            {
                await UniTask.Delay(1000);

                using (var getRequest = UnityWebRequest.Get(url))
                {
                    getRequest.SetRequestHeader("X-Key", $"Key {api_key}");
                    getRequest.SetRequestHeader("X-Secret", $"Secret {secret_key}");

                    await getRequest.SendWebRequest();

                    var imageResult = JsonConvert.DeserializeObject<ImageResult>(getRequest.downloadHandler.text);
                    imageCurrentStatus = imageResult.status;
                    images = imageResult.images;

                    Debug.Log($"poling image: {imageResult.status} - {imageResult.censored}");
                }
            }
            Debug.Log($"poling image done");

            //convert image
            byte[] bytes = Convert.FromBase64String(images[0]);
            Texture2D image = new(0, 0);
            ImageConversion.LoadImage(image, bytes);
            return image;
        }
    }
}

