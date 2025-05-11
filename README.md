# BoosterrCLI
BoosterrCLI is a Custom Formats manager that integrates with both [Radarr](https://github.com/Radarr/Radarr) and [Sonarr](https://github.com/Sonarr/Sonarr).
It consists of the largest known [database of Custom Formats / Release Profile Restrictions](https://github.com/denial-of-service/BoosterrCLI/raw/refs/heads/master/boosterr.xlsx) (900+).
It also includes a console program that automatically syncs the desired Custom Formats with your Radarr and Sonarr instances.

## Prerequisites
- Have [.NET Runtime 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) or higher installed.

## Instructions
1. Clone the repository.
1. Add your Media Manager Instances to the `config.yaml` file.
1. Decide which Terms to sync or ignore by editing the Sync column in the `boosterr.xlsx` file and then saving.
1. Build the project and run the `BoosterrCLI.exe` file.

## Credits
- [Trash Guides](https://trash-guides.info/Radarr/Radarr-collection-of-custom-formats/#index): Served as the inspiration for this project. I have consulted their collection of Custom Formats to complete my own collection. However, the regex was usually altered/rewritten.
- Wikipedia articles: [Common abbreviations for digital platforms](https://en.wikipedia.org/wiki/Pirated_movie_release_types#Common_abbreviations_for_digital_platforms), [Alphabetic country codes](https://en.wikipedia.org/wiki/Comparison_of_alphabetic_country_codes#List), [List of video coding standards](https://en.wikipedia.org/wiki/Video_coding_format#List_of_video_coding_standards) and many more.
- [Scene Lingo](https://scenelingo.wordpress.com/): Collection of abbreviations used in Warez Scene release titles.
- [Scene Rules](https://scenerules.org/): Collection of Warez Scene release standards.
- The wikis, forums and upload rules of several private trackers.
