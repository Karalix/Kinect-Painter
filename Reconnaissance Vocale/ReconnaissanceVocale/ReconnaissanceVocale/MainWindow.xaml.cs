

namespace ReconnaissanceVocale
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Speech.AudioFormat;
    using System.Speech.Recognition;
    using System.Speech.Recognition.SrgsGrammar;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Resource key for medium-gray-colored brush.
        /// </summary>
        private const string MediumGreyBrushKey = "MediumGreyBrush";

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        /// <summary>
        /// List of all UI span elements used to select recognized text.
        /// </summary>
        private List<Span> recognitionSpans;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && ("fr-FR".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase) || "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return recognizer;
                }
            }

            return null;
        }

        /// <summary>
        /// Execute initialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }


                RecognizerInfo ri = GetKinectRecognizer();

                if (null != ri)
                {
                    recognitionSpans = new List<Span> { BrosseSpan, PinceauSpan, CrayonSpan, CubeSpan, SphereSpan, PyramideSpan, ManuelSpan, AutoSpan, NordSpan, SudSpan, EstSpan, OuestSpan };

                    this.speechEngine = new SpeechRecognitionEngine(ri.Id);
                    this.speechEngine.SetInputToDefaultAudioDevice();


                    // Create a grammar programmaticaly without an xml file
                    /*var BaseGram = new Choices();

                    BaseGram.Add(new SemanticResultValue("brosse", "BROSSE"));
                    BaseGram.Add(new SemanticResultValue("default", "BROSSE"));
                    BaseGram.Add(new SemanticResultValue("pinceau", "PINCEAU"));
                    BaseGram.Add(new SemanticResultValue("crayon", "CRAYON"));
                    BaseGram.Add(new SemanticResultValue("sphere", "SPHERE"));
                    BaseGram.Add(new SemanticResultValue("pyramide", "PYRAMIDE"));
                    BaseGram.Add(new SemanticResultValue("cube", "CUBE"));
                    BaseGram.Add(new SemanticResultValue("manuel", "MANUEL"));
                    BaseGram.Add(new SemanticResultValue("automatique", "AUTOMATIQUE"));
                    BaseGram.Add(new SemanticResultValue("auto", "AUTOMATIQUE"));
                    BaseGram.Add(new SemanticResultValue("nord", "NORD"));
                    BaseGram.Add(new SemanticResultValue("sud", "SUD"));
                    BaseGram.Add(new SemanticResultValue("est", "EST"));
                    BaseGram.Add(new SemanticResultValue("este", "EST"));
                    BaseGram.Add(new SemanticResultValue("east", "EST"));
                    BaseGram.Add(new SemanticResultValue("ouest", "OUEST"));

                    var gb = new GrammarBuilder { Culture = ri.Culture };

                    gb.Append(BaseGram);

                    var g = new Grammar(gb);*/


                   /* //Création d'un document de la norme SRGS à partir du fichier grxml
                    SrgsDocument xmlGrammar = new SrgsDocument("GrammarBase.xml");
                    //Création d'une grammaire depuis le fichier de grammaire
                    Grammar grammar = new Grammar(xmlGrammar);

                    speechEngine.LoadGrammar(grammar);*/

                    using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.GrammarBase)))
                    {
                        var g = new Grammar(memoryStream);
                        speechEngine.LoadGrammar(g);
                    }

                    speechEngine.SpeechRecognized += SpeechRecognized;
                    speechEngine.SpeechRecognitionRejected += SpeechRejected;
                    speechEngine.SpeechHypothesized += SpeechHypothesized;

                    // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                    // This will prevent recognition accuracy from degrading over time.
                    ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                    speechEngine.SetInputToAudioStream(
                        sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                    speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                }

            }
        }

        /// <summary>
        /// Execute uninitialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.SpeechHypothesized -= SpeechHypothesized;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        /// <summary>
        /// Remove any highlighting from recognition instructions.
        /// </summary>
        private void ClearRecognitionHighlights()
        {
            foreach (Span span in recognitionSpans)
            {
                span.Foreground = (Brush)this.Resources[MediumGreyBrushKey];
                span.FontWeight = FontWeights.Normal;
            }
        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            ClearRecognitionHighlights();

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "BRUSH":
                        BrosseSpan.Foreground = Brushes.Green;
                        BrosseSpan.FontWeight = FontWeights.Bold;

                        Console.WriteLine("Vous avez dit brosse ! ");

                        // method for brosse
                        //
                        //
                        //

                        break;

                    case "CRAYON":
                        CrayonSpan.Foreground = Brushes.Green;
                        CrayonSpan.FontWeight = FontWeights.Bold;

                        // method for crayon
                        // 
                        //
                        //
                           
                        break;

                    case "PINCEAU":
                        PinceauSpan.Foreground = Brushes.Green;
                        PinceauSpan.FontWeight = FontWeights.Bold;

                        // method for pinceau
                        //
                        //
                        //

                        break;

                    case "SPHERE":
                        SphereSpan.Foreground = Brushes.Green;
                        SphereSpan.FontWeight = FontWeights.Bold;

                        // method for sphere
                        //
                        //
                        //

                        break;

                    case "CUBE":
                        CubeSpan.Foreground = Brushes.Green;
                        CubeSpan.FontWeight = FontWeights.Bold;

                        // method for cube
                        //
                        //
                        //

                        break;

                    case "PYRAMIDE":
                        PyramideSpan.Foreground = Brushes.Green;
                        PyramideSpan.FontWeight = FontWeights.Bold;

                        // method for pyramide
                        //
                        //
                        //

                        break;

                    case "MANUEL":
                        ManuelSpan.Foreground = Brushes.Green;
                        ManuelSpan.FontWeight = FontWeights.Bold;

                        // method for manuel
                        //
                        //
                        //

                        break;

                    case "AUTOMATIQUE":
                        AutoSpan.Foreground = Brushes.Green;
                        AutoSpan.FontWeight = FontWeights.Bold;

                        // method for automatique
                        //
                        //
                        //

                        break;

                    case "NORD":
                        NordSpan.Foreground = Brushes.Green;
                        NordSpan.FontWeight = FontWeights.Bold;

                        // method for nord
                        //
                        //
                        //

                        break;

                    case "SUD":
                        SudSpan.Foreground = Brushes.Green;
                        SudSpan.FontWeight = FontWeights.Bold;

                        // method for sud
                        //
                        //
                        //

                        break;

                    case "OUEST":
                        OuestSpan.Foreground = Brushes.Green;
                        OuestSpan.FontWeight = FontWeights.Bold;

                        // method for ouest
                        //
                        //
                        //

                        break;

                    case "EST":
                        EstSpan.Foreground = Brushes.Green;
                        EstSpan.FontWeight = FontWeights.Bold;

                        // method for est
                        //
                        //
                        //

                        break;
                }
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            ClearRecognitionHighlights();
        }

        // <summary>
        /// Handler for in progress speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {

        }
    }
}
