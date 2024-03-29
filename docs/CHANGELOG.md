## [3.0.18](https://github.com/Faustvii/zlib-torznab/compare/v3.0.17...v3.0.18) (2023-01-27)


### Bug Fixes

* fixed libgen indexer querying fiction section ([f72f1bd](https://github.com/Faustvii/zlib-torznab/commit/f72f1bd170cb2e000b67424f23ada140b49aea12))

## [3.0.17](https://github.com/Faustvii/zlib-torznab/compare/v3.0.16...v3.0.17) (2023-01-27)


### Bug Fixes

* running multiple indexers at the same time if first one doesn't finish before next schedule ([e2de92b](https://github.com/Faustvii/zlib-torznab/commit/e2de92b8b4b3e23ef3b217e59fb12bf3a8c7cda3))

## [3.0.16](https://github.com/Faustvii/zlib-torznab/compare/v3.0.15...v3.0.16) (2023-01-16)


### Bug Fixes

* single character words would not match elastic query ([616c637](https://github.com/Faustvii/zlib-torznab/commit/616c6377380fd67f31c94050fbf3566ffa1c532f))

## [3.0.15](https://github.com/Faustvii/zlib-torznab/compare/v3.0.14...v3.0.15) (2023-01-11)


### Bug Fixes

* fixed indexing missing entries ([a1382a5](https://github.com/Faustvii/zlib-torznab/commit/a1382a53f7eec1590b7b8411929e3162e27f3c9f))

## [3.0.14](https://github.com/Faustvii/zlib-torznab/compare/v3.0.13...v3.0.14) (2023-01-09)


### Bug Fixes

* do some basic size check in case of partial db dump downloads ([68295e7](https://github.com/Faustvii/zlib-torznab/commit/68295e7bc09443eb9afeae1255fffd5b49dbab29))

## [3.0.13](https://github.com/Faustvii/zlib-torznab/compare/v3.0.12...v3.0.13) (2023-01-03)


### Bug Fixes

* cleaning issue of libgen fiction ([9b3ae2d](https://github.com/Faustvii/zlib-torznab/commit/9b3ae2daf35b385ce554b43f50d56369e3b6549d))

## [3.0.12](https://github.com/Faustvii/zlib-torznab/compare/v3.0.11...v3.0.12) (2022-12-24)


### Bug Fixes

* remove temp downloaded files once seeding is done ([e6d7b82](https://github.com/Faustvii/zlib-torznab/commit/e6d7b82f74b9984afe273d8c43a4ba25fada2b7f))

## [3.0.11](https://github.com/Faustvii/zlib-torznab/compare/v3.0.10...v3.0.11) (2022-12-24)


### Bug Fixes

* elastic search was way too fuzzy ([62ebdd6](https://github.com/Faustvii/zlib-torznab/commit/62ebdd6722f7841e2cfb3fd749a868e8744e7ce2))

## [3.0.10](https://github.com/Faustvii/zlib-torznab/compare/v3.0.9...v3.0.10) (2022-12-24)


### Bug Fixes

* error during fiction import ([9706a13](https://github.com/Faustvii/zlib-torznab/commit/9706a13f78a71c2926d5ee6bf9a7bd4d05ee7ba4))
* index usage for indexing queries ([767c905](https://github.com/Faustvii/zlib-torznab/commit/767c905cf14091113f6734c0eb4eaecf935806c2))

## [3.0.9](https://github.com/Faustvii/zlib-torznab/compare/v3.0.8...v3.0.9) (2022-12-24)


### Bug Fixes

* missed another index ([7e5b755](https://github.com/Faustvii/zlib-torznab/commit/7e5b755a22f9b49de065c11c6bd2d99b65e132c2))

## [3.0.8](https://github.com/Faustvii/zlib-torznab/compare/v3.0.7...v3.0.8) (2022-12-24)


### Bug Fixes

* improved metadata import ([2bd4dd6](https://github.com/Faustvii/zlib-torznab/commit/2bd4dd61c76d94de3971e5d03c421346559bb5a8))

## [3.0.7](https://github.com/Faustvii/zlib-torznab/compare/v3.0.6...v3.0.7) (2022-12-24)


### Bug Fixes

* guard against dropping or renaming if import didn't go well ([b0ed000](https://github.com/Faustvii/zlib-torznab/commit/b0ed000dee4cf8ad494db551b9c7fb2ad3e001f2))
* running multiple imports at same time.. ([e87759b](https://github.com/Faustvii/zlib-torznab/commit/e87759b95b916b73dbbf0e689b135ecdf40fb7a2))

## [3.0.6](https://github.com/Faustvii/zlib-torznab/compare/v3.0.5...v3.0.6) (2022-12-24)


### Bug Fixes

* hosted services crashing app on exception, now they will be logged ([828e51d](https://github.com/Faustvii/zlib-torznab/commit/828e51d674c18a438bb7c49e5e7e76d98a10b7a4))

## [3.0.5](https://github.com/Faustvii/zlib-torznab/compare/v3.0.4...v3.0.5) (2022-12-17)


### Bug Fixes

* we should overwrite the file when we know it already exists.. ([28240eb](https://github.com/Faustvii/zlib-torznab/commit/28240ebdb82ea65de9334e4d8fd000732b701555))

## [3.0.4](https://github.com/Faustvii/zlib-torznab/compare/v3.0.3...v3.0.4) (2022-12-17)


### Bug Fixes

* increase command timeout on indexing queries ([7b0f564](https://github.com/Faustvii/zlib-torznab/commit/7b0f56415d7c571324f31dc01ada62f808186aca))

## [3.0.3](https://github.com/Faustvii/zlib-torznab/compare/v3.0.2...v3.0.3) (2022-12-17)


### Bug Fixes

* additional processing of libgen sql ([73e0a5f](https://github.com/Faustvii/zlib-torznab/commit/73e0a5fcacfaeb8465d5b8e0de464260a45c356c))

## [3.0.2](https://github.com/Faustvii/zlib-torznab/compare/v3.0.1...v3.0.2) (2022-12-16)


### Bug Fixes

* elastic indexing would sometimes index data from the past when it shouldn't ([f818a57](https://github.com/Faustvii/zlib-torznab/commit/f818a57231517dae1fdbda075188faae4f1cbc30))

## [3.0.1](https://github.com/Faustvii/zlib-torznab/compare/v3.0.0...v3.0.1) (2022-12-12)


### Bug Fixes

* elastic index condition for new items was wrong ([54819f8](https://github.com/Faustvii/zlib-torznab/commit/54819f89da394a62da3c527ede27c1484f084133))

# [3.0.0](https://github.com/Faustvii/zlib-torznab/compare/v2.1.9...v3.0.0) (2022-12-10)


* feat!: Added Elastic search as a the search engine instead of mysql ([0b0bb9b](https://github.com/Faustvii/zlib-torznab/commit/0b0bb9b5d38267f242c57e1179bb660a8a4a6e48))


### BREAKING CHANGES

* Requires new elastic config setting

## [2.1.9](https://github.com/Faustvii/zlib-torznab/compare/v2.1.8...v2.1.9) (2022-12-05)


### Bug Fixes

* fixed rss feed not really working as readarr would expect ([53caa17](https://github.com/Faustvii/zlib-torznab/commit/53caa176b43b79c74c19997d41170a01a9816808))

## [2.1.8](https://github.com/Faustvii/zlib-torznab/compare/v2.1.7...v2.1.8) (2022-12-05)


### Bug Fixes

* ordered the index incorrectly so it wasn't used... ([fe26464](https://github.com/Faustvii/zlib-torznab/commit/fe264645fd60897d9666cca12aa904000a29986e))

## [2.1.7](https://github.com/Faustvii/zlib-torznab/compare/v2.1.6...v2.1.7) (2022-12-05)


### Bug Fixes

* tables being locked while importing data ([27b470a](https://github.com/Faustvii/zlib-torznab/commit/27b470a5fdbe58ab0512deb3c0aea3475b66e500))

## [2.1.6](https://github.com/Faustvii/zlib-torznab/compare/v2.1.5...v2.1.6) (2022-12-03)


### Bug Fixes

* the stored md5 is unreliable, switch to ipfs cid for torznab guid ([dc64559](https://github.com/Faustvii/zlib-torznab/commit/dc645596ae766a4c3442eda6449521efe7bd4b0c))

## [2.1.5](https://github.com/Faustvii/zlib-torznab/compare/v2.1.4...v2.1.5) (2022-12-03)


### Bug Fixes

* validate expected filesize with actual filesize ([8d6ed6d](https://github.com/Faustvii/zlib-torznab/commit/8d6ed6d58172a015e9fd8fd1d6fbebfdbbc5a1a2))

## [2.1.4](https://github.com/Faustvii/zlib-torznab/compare/v2.1.3...v2.1.4) (2022-12-03)


### Bug Fixes

* leaving partially downloaded files when cancellation requested ([245ee88](https://github.com/Faustvii/zlib-torznab/commit/245ee88b7557ca4ea8bc3bae1521808adb0b4bfd))

## [2.1.3](https://github.com/Faustvii/zlib-torznab/compare/v2.1.2...v2.1.3) (2022-12-03)


### Bug Fixes

* fixed query timeout because querying on a coalesced column ([399857a](https://github.com/Faustvii/zlib-torznab/commit/399857a738f843302a9672d51ee397818989351a))

## [2.1.2](https://github.com/Faustvii/zlib-torznab/compare/v2.1.1...v2.1.2) (2022-12-03)


### Bug Fixes

* added missing lookup when trying to download torrent for zlibrary book ([9a4776f](https://github.com/Faustvii/zlib-torznab/commit/9a4776f68044bbc082b0c3ada9fb88ba3a7febee))

## [2.1.1](https://github.com/Faustvii/zlib-torznab/compare/v2.1.0...v2.1.1) (2022-12-02)


### Bug Fixes

* check if file exists after downloading the file from ipfs ([4efb158](https://github.com/Faustvii/zlib-torznab/commit/4efb158ee127b640b2112d92afc078c87cb143ba))

# [2.1.0](https://github.com/Faustvii/zlib-torznab/compare/v2.0.0...v2.1.0) (2022-12-02)


### Features

* added zlibrary data querying ([02679d9](https://github.com/Faustvii/zlib-torznab/commit/02679d9a5f089a29f9afb128ce3a04a7a71c220f))

# [2.0.0](https://github.com/Faustvii/zlib-torznab/compare/v1.5.8...v2.0.0) (2022-12-02)


* feat!: auto update of libgen data ([3d2b0e1](https://github.com/Faustvii/zlib-torznab/commit/3d2b0e126a85f23873b818eb1137d7e5383817f1))


### BREAKING CHANGES

* Requires new metadata config setting

## [1.5.8](https://github.com/Faustvii/zlib-torznab/compare/v1.5.7...v1.5.8) (2022-11-29)


### Bug Fixes

* removed q torznab capability ([1ea1c3c](https://github.com/Faustvii/zlib-torznab/commit/1ea1c3cab705133e42f425a3f921fe18f297b715))

## [1.5.7](https://github.com/Faustvii/zlib-torznab/compare/v1.5.6...v1.5.7) (2022-11-29)


### Bug Fixes

* preloading was using old ipfs client to download ([fd5dbd7](https://github.com/Faustvii/zlib-torznab/commit/fd5dbd7fd325a964c06f58b19b91a143743d310b))

## [1.5.6](https://github.com/Faustvii/zlib-torznab/compare/v1.5.5...v1.5.6) (2022-11-29)


### Bug Fixes

* tried to read book with incorrect filename ([cabf081](https://github.com/Faustvii/zlib-torznab/commit/cabf0814be3e15af888630c4e82ce1bd7b624df7))

## [1.5.5](https://github.com/Faustvii/zlib-torznab/compare/v1.5.4...v1.5.5) (2022-11-29)


### Bug Fixes

* torrent filename being invalid for readarr to import ([206ddbe](https://github.com/Faustvii/zlib-torznab/commit/206ddbec57c783a4014122562c86a053fe993d8f))

## [1.5.4](https://github.com/Faustvii/zlib-torznab/compare/v1.5.3...v1.5.4) (2022-11-29)


### Bug Fixes

* better connetion failed log ([5bf6244](https://github.com/Faustvii/zlib-torznab/commit/5bf6244513b1a822a55c19fa5cf868b086bca27d))

## [1.5.3](https://github.com/Faustvii/zlib-torznab/compare/v1.5.2...v1.5.3) (2022-11-29)


### Bug Fixes

* added debugging to torrent manager ([dbbf8c9](https://github.com/Faustvii/zlib-torznab/commit/dbbf8c933c817a3520ee1aafea9c3443397853d2))

## [1.5.2](https://github.com/Faustvii/zlib-torznab/compare/v1.5.1...v1.5.2) (2022-11-29)


### Bug Fixes

* torrent already registered error ([f119c38](https://github.com/Faustvii/zlib-torznab/commit/f119c388a88e8bd44cb3470c472756e2d8504d49))

## [1.5.1](https://github.com/Faustvii/zlib-torznab/compare/v1.5.0...v1.5.1) (2022-11-29)


### Bug Fixes

* added support for cloudfare ipfs gateway ([5982108](https://github.com/Faustvii/zlib-torznab/commit/59821084ccf13e25516bd63e5c46e054fc1b5adb))

# [1.5.0](https://github.com/Faustvii/zlib-torznab/compare/v1.4.1...v1.5.0) (2022-11-28)


### Features

* added queue download from ipfs if only one result ([153a0b5](https://github.com/Faustvii/zlib-torznab/commit/153a0b5b33ad9dc925d04583326f2d96c3e5e685))

## [1.4.1](https://github.com/Faustvii/zlib-torznab/compare/v1.4.0...v1.4.1) (2022-11-28)


### Bug Fixes

* improved query so all "words" must exist in author and title when doing fulltext ([737b60f](https://github.com/Faustvii/zlib-torznab/commit/737b60fb7697eb590bc5876921ac2b9556209839))
* publisher was mapped to pages ([3265f3d](https://github.com/Faustvii/zlib-torznab/commit/3265f3da298c0ae2a0cff9c4409cfac8ecc8854e))

# [1.4.0](https://github.com/Faustvii/zlib-torznab/compare/v1.3.0...v1.4.0) (2022-11-28)


### Features

* filter books by english language ([fa1020b](https://github.com/Faustvii/zlib-torznab/commit/fa1020b3299ef73621f22041198a9cd571523111))

# [1.3.0](https://github.com/Faustvii/zlib-torznab/compare/v1.2.0...v1.3.0) (2022-11-28)


### Features

* added libgen data and possibility to search both datasets ([aa2c2f2](https://github.com/Faustvii/zlib-torznab/commit/aa2c2f26f6d22908d4fd4cafce1c1cc7b47ac46a))
* switched to md5 as identifier instead of ipfs cid ([a69060d](https://github.com/Faustvii/zlib-torznab/commit/a69060d40da691dc81d27cb0628facd314be0b8b))
* use fulltext indexes for searching ([9a58ca9](https://github.com/Faustvii/zlib-torznab/commit/9a58ca98312662b3141053d7f2c96a9238f5e409))

# [1.2.0](https://github.com/Faustvii/zlib-torznab/compare/v1.1.0...v1.2.0) (2022-11-27)


### Bug Fixes

* try add task delay to see if all interfaces will be listed. ([b74979e](https://github.com/Faustvii/zlib-torznab/commit/b74979ef0d1f3ed331d149a8e9ee0dc026b74ebb))


### Features

* added ip translator ([cecd30d](https://github.com/Faustvii/zlib-torznab/commit/cecd30d976b320053a501091b9109cabd4602ca3))

# [1.1.0](https://github.com/Faustvii/zlib-torznab/compare/v1.0.1...v1.1.0) (2022-11-27)


### Features

* added ability to bind monotorrent to specific interface ([e79a0b9](https://github.com/Faustvii/zlib-torznab/commit/e79a0b9b9e1055de96f8c35e318b9d808fadc281))

## [1.0.1](https://github.com/Faustvii/zlib-torznab/compare/v1.0.0...v1.0.1) (2022-11-27)


### Bug Fixes

* performance issue because ordering on non indexed column. ([4467a0a](https://github.com/Faustvii/zlib-torznab/commit/4467a0aad2fdf7350dbfb1cecc1caae5582a814d))

# 1.0.0 (2022-11-27)


### Features

* added first prototype version ([2176ca1](https://github.com/Faustvii/zlib-torznab/commit/2176ca19350f1951ed08635f308e1fa57724988a))
