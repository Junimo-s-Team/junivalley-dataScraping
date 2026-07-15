﻿using PuppeteerSharp;
using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;
using System.Collections.Generic;

await ProcessVillagers();

async Task ProcessVillagers()
{
    List<VillagerModel> villagersFailure = new List<VillagerModel>();
    var jobs = new List<(VillagerModel villager, string languagePrefix)>();

    // 1. Preparamos todos los trabajos de scraping
    foreach (var villager in GeneralConstants.VILLAGERS)
    {
        foreach (var languagePrefix in GeneralConstants.LANGUAGES)
        {
            jobs.Add((villager, languagePrefix));
        }
    }

    // 2. Descargamos el navegador una sola vez
    using var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    // 3. Procesamos los trabajos en paralelo con un límite de concurrencia
    await Parallel.ForEachAsync(jobs, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (job, token) =>
    {
        var (villager, languagePrefix) = job;
        int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            IBrowser? browser = null;
            try
            {
                browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                var manager = new VillagerManager(browser);

                string languageCode = languagePrefix.Replace(".", "").ToUpper();
                if (string.IsNullOrEmpty(languageCode)) languageCode = "EN";

                string villagerNameForUrl = VillagerLanguageNames.GetTranslatedName(villager.Name, languageCode);
                string url = $"https://{languagePrefix.ToLower()}{GeneralConstants.BASE_URL}/{villagerNameForUrl}";

                VillagerModel villagerData = await manager.GetVillagerPrimaryData(url, villager, languageCode);
                manager.ConvertToJson(villagerData, villagerData.Language);
                Console.WriteLine($"[ÉXITO] Guardado: {villager.Name} en idioma {villagerData.Language}");
                break; // Éxito, salimos del bucle de reintentos
            }
            catch (TargetClosedException ex)
            {
                Console.WriteLine($"[ERROR] TargetClosedException con {villager.Name} ({languagePrefix}). Intento {attempt}/{maxRetries}. Reintentando... Error: {ex.Message}");
                if (attempt == maxRetries) villagersFailure.Add(villager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Algo ha ido mal con {villager.Name} ({languagePrefix}). Excepción: {ex.Message}");
                villagersFailure.Add(villager);
                break; // Si es otro tipo de error, no reintentamos para este aldeano/idioma.
            }
            finally
            {
                if (browser != null) await browser.CloseAsync();
            }
        }
    });
}