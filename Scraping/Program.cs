﻿﻿﻿using PuppeteerSharp;
using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;
using System.Collections.Generic;

while (true)
{
    Console.WriteLine("\n--- Stardew Valley Scraper ---");
    Console.WriteLine("¿Qué te gustaría extraer?");
    Console.WriteLine("  1. Datos de Aldeanos (Villagers)");
    Console.WriteLine("  2. Datos de Misiones (Quests)");
    Console.WriteLine("  3. Datos de la Casa de Campo (Farmhouse)");
    Console.WriteLine("  4. Datos de Animales (Animals)");
    Console.WriteLine("  5. Extraer todo");
    Console.WriteLine("  6. Salir");
    Console.Write("Elige una opción y pulsa Intro: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await ProcessVillagers();
            break;
        case "2":
            await ProcessQuests();
            break;
        case "3":
            await ProcessFarmhouse();
            break;
        case "4":
            await ProcessAnimals();
            break;
        case "5":
            await ProcessQuests();
            await ProcessVillagers();
            await ProcessFarmhouse();
            await ProcessAnimals();
            break;
        case "6":
            Console.WriteLine("Saliendo del programa...");
            return;
        default:
            Console.WriteLine("Opción no válida. Por favor, elige un número del 1 al 6.");
            break;
    }
}

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

async Task ProcessQuests()
{
    Console.WriteLine("--- Iniciando Scraper de Misiones ---");
    IBrowser? browser = null;
    try
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        var manager = new QuestsManager(browser);

        foreach (var lang in GeneralConstants.LANGUAGES)
        {
            string langCode = string.IsNullOrEmpty(lang) ? "EN" : lang.Replace(".", "").ToUpper();
            Console.WriteLine($"Extrayendo misiones para el idioma: {langCode}");
            await manager.ScrapeAndSaveQuests(langCode);
            Console.WriteLine($"[ÉXITO] Misiones guardadas para el idioma {langCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR FATAL] El scraper de misiones falló: {ex.Message}");
    }
    finally
    {
        if (browser != null) await browser.CloseAsync();
        Console.WriteLine("--- Scraper de Misiones Finalizado ---");
    }
}

async Task ProcessFarmhouse()
{
    Console.WriteLine("--- Iniciando Scraper de la Casa de Campo ---");
    IBrowser? browser = null;
    try
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        var manager = new FarmhouseManager(browser);

        foreach (var lang in GeneralConstants.LANGUAGES)
        {
            string langCode = string.IsNullOrEmpty(lang) ? "EN" : lang.Replace(".", "").ToUpper();
            Console.WriteLine($"Extrayendo datos de la casa de campo para el idioma: {langCode}");
            await manager.ScrapeAndSaveFarmhouse(langCode);
            Console.WriteLine($"[ÉXITO] Datos de la casa de campo guardados para el idioma {langCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR FATAL] El scraper de la casa de campo falló: {ex.Message}");
    }
    finally
    {
        if (browser != null) await browser.CloseAsync();
        Console.WriteLine("--- Scraper de la Casa de Campo Finalizado ---");
    }
}

async Task ProcessAnimals()
{
    Console.WriteLine("--- Iniciando Scraper de Animales ---");
    IBrowser? browser = null;
    try
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        var manager = new AnimalsManager(browser);

        foreach (var lang in GeneralConstants.LANGUAGES)
        {
            string langCode = string.IsNullOrEmpty(lang) ? "EN" : lang.Replace(".", "").ToUpper();
            Console.WriteLine($"Extrayendo datos de animales para el idioma: {langCode}");
            await manager.ScrapeAndSaveAnimals(langCode);
            Console.WriteLine($"[ÉXITO] Datos de animales guardados para el idioma {langCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR FATAL] El scraper de animales falló: {ex.Message}");
    }
    finally
    {
        if (browser != null) await browser.CloseAsync();
        Console.WriteLine("--- Scraper de Animales Finalizado ---");
    }
}