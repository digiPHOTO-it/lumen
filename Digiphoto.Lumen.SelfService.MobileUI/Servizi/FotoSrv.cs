﻿using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.SelfService.MobileUI.Servizi
{
    public class FotoSrv
    {
        private static FotoSrv instance;

        private readonly String selfPath = @"C:\Self\";
        private String proviniPath = "Provini";
        private String risultantePath;
        private Guid idCarrello;

        private FotoSrv()
        {
            // Creo la cartella che conterrà le foto
            if (System.IO.Directory.Exists(selfPath))
            {
                deleteDirectory(selfPath);
            }
            if (!System.IO.Directory.Exists(selfPath))
            {
                Directory.CreateDirectory(selfPath);
            }
        }

        public static FotoSrv Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FotoSrv();
                }
                return instance;
            }
        }

        public void setCarello(Guid id)
        {
            if (!idCarrello.Equals(id))
            {
                if (!idCarrello.Equals(Guid.Empty))
                {
                    String pathCarrelloOld = Path.Combine(selfPath, idCarrello.ToString());
                    deleteDirectory(pathCarrelloOld);
                }
                String pathCarrelloNew = Path.Combine(selfPath, id.ToString());
                String pathProvini = Path.Combine(pathCarrelloNew, proviniPath);

                Directory.CreateDirectory(pathCarrelloNew);
                Directory.CreateDirectory(pathProvini);

                risultantePath = pathCarrelloNew;
                idCarrello = id;
            }
        }

        public BitmapImage loadPhoto(SelfServiceClient ssClient, string quale, Guid fotografiaId)
        {
            byte[] bytes = null;
            bool scriviFile = false;
            String photoName = fotografiaId.ToString();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            String _path = Path.Combine(selfPath, System.IO.Path.ChangeExtension(photoName, ".jpg"));

            if (quale == "Provino")
            {    
                _path = Path.Combine(risultantePath, proviniPath, System.IO.Path.ChangeExtension(photoName, ".jpg"));
                if (!System.IO.File.Exists(_path))
                {
                    bytes = ssClient.getImageProvino(fotografiaId);
                    scriviFile = true;
                }
            }
            else if (quale == "Logo")
            {
                _path = Path.Combine(selfPath, System.IO.Path.ChangeExtension("Logo", ".jpg"));
                if (!System.IO.File.Exists(_path))
                {
                    bytes = ssClient.getImageLogo();
                    scriviFile = true;
                }
            }
            else if (quale == "Risultante")
            {
                _path = Path.Combine(risultantePath, System.IO.Path.ChangeExtension(photoName, ".jpg"));
                if (!System.IO.File.Exists(_path))
                {
                    bytes = ssClient.getImage(fotografiaId);
                    scriviFile = true;
                }
            }
            
            // Salvo il file su disco
            if (scriviFile)
            {
                File.WriteAllBytes(_path, bytes);
            }
            BitmapImage tempImage = new BitmapImage();
            try
            {
                tempImage.BeginInit();
                tempImage.UriSource = new Uri(_path);
                //tempImage.DecodePixelWidth = 1800;
                //tempImage.DecodePixelWidth = 600;
                tempImage.CacheOption = BitmapCacheOption.OnLoad;
                tempImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                tempImage.EndInit();
                tempImage.Freeze();
            }
            catch (Exception ex) { ex.ToString(); }
            sw.Stop();
            Console.WriteLine("File={0} - ScriviFile={1} - Elapsed={2}", _path, scriviFile, sw.Elapsed);
            return tempImage;
        }

        /// <summary>
        /// Consente di eliminare tutte le foto contenute nel path
        /// </summary>
        /// <param name="pathCartella"></param>
        public void deleteDirectory(String pathCartella)
        {
            foreach (string directoryPath in Directory.GetFiles(pathCartella))
            {
                String nomeFile = Path.GetFileName(directoryPath);
                //Elimino gli attributi solo lettura file nascosti
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~FileAttributes.Hidden);
                //Elimino gli attributi solo lettura                                
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));
            }
            String dirDataPath = Path.GetDirectoryName(pathCartella);
            Directory.Delete(pathCartella, true);
        }

    }
}