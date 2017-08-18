﻿// <copyright file="StartPage.cs" company="Esri">
//      Copyright (c) 2017 Esri. All rights reserved.
//
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>

namespace MapsApp
{
    using Esri.ArcGISRuntime.Symbology;
    using Esri.ArcGISRuntime.UI;
    using MapsApp.Shared.ViewModels;
    using System;
    using System.IO;
    using System.Reflection;
    using Xamarin.Forms;

    public partial class StartPage : ContentPage
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="StartPage"/> class.
        /// </summary>
        public StartPage()
		{
			InitializeComponent();

            var geocodeViewModel = Resources["GeocodeViewModel"] as GeocodeViewModel;
            geocodeViewModel.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(GeocodeViewModel.Place):
                        {
                            var place = geocodeViewModel.Place;
                            if (place == null)
                                return;

                            var graphicsOverlay = MapView.GraphicsOverlays["PlacesOverlay"];
                            graphicsOverlay?.Graphics.Clear();

                            // create map pin and add it to the map
                            try
                            {
                                Assembly assembly = typeof(StartPage).GetTypeInfo().Assembly;

                                string imagePath = null;
                                switch (Device.RuntimePlatform)
                                {
                                    case Device.iOS:
                                        imagePath = "MapsApp.iOS.Images.End72.png";
                                        break;
                                    case Device.Android:
                                        imagePath = "MapsApp.Droid.Images.End72.png";
                                        break;
                                    case Device.Windows:
                                        imagePath = "MapsApp.UWP.Images.End72.png";
                                        break;
                                }

                                using (Stream stream = assembly.GetManifestResourceStream(imagePath))
                                {
                                    long length = stream.Length;
                                    var imageData = new byte[length];
                                    stream.Read(imageData, 0, (int)length);

                                    if (imageData != null)
                                    {
                                        var mapPin = new PictureMarkerSymbol(new RuntimeImage(imageData));
                                        var graphic = new Graphic(geocodeViewModel.Place.DisplayLocation, mapPin);
                                        graphicsOverlay?.Graphics.Add(graphic);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Display error message 
                                DisplayAlert("Error", ex.ToString(), "OK");
                            }
                            break;
                        }

                    case nameof(GeocodeViewModel.AreaOfInterest):
                        {
                            CompassImage.Rotation = 360 - this.MapView.MapRotation;
                            break;
                        }

                    case nameof(GeocodeViewModel.IsTopBannerVisible):
                        {
                            // clear map pin when user clears search
                            if (geocodeViewModel.IsTopBannerVisible == false)
                            {
                                var graphicsOverlay = MapView.GraphicsOverlays["PlacesOverlay"];
                                graphicsOverlay?.Graphics.Clear();
                            }
                            break;
                        }
                    case nameof(GeocodeViewModel.ErrorMessage):
                        {
                            // display error message from viewmodel
                            DisplayAlert("Error", geocodeViewModel.ErrorMessage, "OK");
                            break;
                        }
                }
            };

            // start location services
            var mapViewModel = Resources["MapViewModel"] as MapViewModel;
            MapView.LocationDisplay.DataSource = mapViewModel.LocationDataSource;
            MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            MapView.LocationDisplay.IsEnabled = true;

            // change viewpoint to current location
            mapViewModel.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(mapViewModel.AreaOfInterest):
                        {
                            geocodeViewModel.AreaOfInterest = mapViewModel.AreaOfInterest;
                            break;
                        }
                }
            };
        }

        /// <summary>
        /// Resets map rotation to North up
        /// </summary>
        private async void ResetMapRotation(object sender, EventArgs e)
        {
            await this.MapView.SetViewpointRotationAsync(0).ConfigureAwait(false);
        }
    }
}