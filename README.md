# Rekopy

**Rekopy** is a simple tool which allows you to:

1. Import an existing **Rekordbox** collection (exported as XML)
2. Select **all** or **a subset of playlists** in the collection
3. Export those playlists as a **new** XML Rekordbox collection **together with all** the **audio files** in those playlists

When exporting a collection as XML from Rekordbox, **only the database** is exported and the **audio file paths are also absolute**, which means it's nearly impossible to use the exported collection on another device. 

Rekopy solves this issue by making the **file paths relative** and **copying the audio files alongside the XML database**.

Rekopy is designed primarily to work with [Vinyl Reality Lite](https://www.meta.com/experiences/5812678618774066/) (a VR DJ application).


## Compatible Software

### Vinyl Reality Lite

The resulting Rekordbox collection can be used in Vinyl Reality Lite. Being able to load a Rekordbox collection into Vinyl Reality Lite has many advantages, including:

- All meta data can be read directly from the Rekordbox collection, including the BPM and Key values
- Folder hierarchy and playlists are synced between Rekordbox and Vinyl Reality
- Only one music collection needs to be managed and all analysis can be done inside Rekordbox

## Instructions

For instructions on how to use **Rekopy** with **Vinyl Reality Lite** please check out the relevant Vinyl Reality Lite Documentation:

https://vinyl-reality.com/wiki/doku.php?id=vinyl_reality_lite_rekordbox