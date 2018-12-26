using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace FaceDetection
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        const string subscriptionKey = "202631d0172a48d08f21b52bc22ee860";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";
        Bitmap mBitmap;
        private ImageView imageView;
        private ProgressBar progressBar;
        ByteArrayContent content;
        private TextView txtAge, txtGender, txtFaces;
        Button btnAnalyze;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            mBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.newAhsan);
            imageView = FindViewById<ImageView>(Resource.Id.imgView);
            imageView.SetImageBitmap(mBitmap);
            txtGender = FindViewById<TextView>(Resource.Id.txtGender);
            txtAge = FindViewById<TextView>(Resource.Id.txtAge);
            txtFaces = FindViewById<TextView>(Resource.Id.txtFaces);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            btnAnalyze = FindViewById<Button>(Resource.Id.btnAnalyze);
            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                mBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                bitmapData = stream.ToArray();
            }
            content = new ByteArrayContent(bitmapData);

            btnAnalyze.Click += async delegate
            {
                busy();
                await MakeAnalysisRequest(content);
            };
        }
        public async Task MakeAnalysisRequest(ByteArrayContent content)
        {
            try
            {
                HttpClient client = new HttpClient();
                // Request headers.
                client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters = "returnFaceId=true&returnfaceRectangle=true" +
                                          "&returnFaceAttributes=age,gender,smile,glasses";
                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                var response = await client.PostAsync(uri, content);

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                var faces = JsonConvert.DeserializeObject<List<AnalysisModel>>(contentString);
                Toast.MakeText(this, faces.Count.ToString() + " Face Detected", ToastLength.Short).Show();
                NotBusy();
                var newbitmap = DrawRectanglesOnBitmap(mBitmap, faces);
                imageView.SetImageBitmap(newbitmap);
                txtGender.Text = "Gender:  " + faces[0].faceAttributes.gender.ToString();
                txtAge.Text = "Age:  " + faces[0].faceAttributes.age.ToString();
                txtFaces.Text = "Glasses:  " + faces[0].faceAttributes.glasses.ToString();
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "" + e.ToString(), ToastLength.Short).Show();
            }
        }

        private Bitmap DrawRectanglesOnBitmap(Bitmap mybitmap, List<AnalysisModel> faces)
        {
            Bitmap bitmap = mybitmap.Copy(Bitmap.Config.Argb8888, true);
            Canvas canvas = new Canvas(bitmap);
            Paint paint = new Paint();
            paint.AntiAlias = true;
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Color.DodgerBlue;
            paint.StrokeWidth = 12;
            foreach (var face in faces)
            {
                var faceRectangle = face.faceRectangle;
                canvas.DrawRect(faceRectangle.left,
                    faceRectangle.top,
                    faceRectangle.left + faceRectangle.width,
                    faceRectangle.top + faceRectangle.height, paint);
            }
            return bitmap;
        }
        void busy()
        {
            progressBar.Visibility = ViewStates.Visible;
            btnAnalyze.Enabled = false;
        }
        void NotBusy()
        {
            progressBar.Visibility = ViewStates.Invisible;
            btnAnalyze.Enabled = true;
        }
    }
}

