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
