[![Azin-DevOps: Semantic-Release](https://img.shields.io/badge/Azin--DevOps-Semantic--Release-informational.svg?style=for-the-badge&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAIAAACQKrqGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD7SURBVChTY/j37/+3H38+ff2FBwEV/Pv3j+Hzt1+HLrxYvPPOwu23sSKg1MELL4DKGF69/z59/Y2k9iOxzYewoqT2w1PXXQcqAymduPpqZMOBwKq97kU73ZAQkBtQuTei4UD/yqtQpZNWXw2r3Z/Ve+w/BsibcCK0dn//qisoStO6j0LlkQBQ/2BUmonNW7n9x1GUAgMrov4AELUuutgw7zwcAbnAQAyvP9C/ChxYL999n7zmWkzTwbC6/cCgRUNAweimg0CzQEo/fP656cij7uWXO5Zcwoq6l13eePgRUBnDn7//3n768eT11yevvoJINAQWfPvxx5+//wCvpcSv3A4ENwAAAABJRU5ErkJggg==)](https://azintelecomgroup.slack.com/archives/C0333KYCH6J)

> Semantic-Release Cədvəli


| COMMIT MESSAGE | RELEASE TYPE |
| ------ | ------ |
| fix(`Fiksin predmeti`): `description` | Patch / Fix Release |
| feat(`Feature predmeti`): `description` | Minor / Feature Release |
| perf(`Performance update predmeti`): `description` | Major / Breaking Release |
BREAKING CHANGE: Bu açar sözü şərtidir, `perf update`-in footerində eyni bu 
formatda (`BREAKING CHANGE`: `description`) yazıla bilər.

> Version Rollback

Müəyyən vM.m.p versiyasının `master`-də geri rollback olunması üçün:

- Soldakı paneldən CI/CD bölməsinə daxil oluruq.
- "Run pipeline" düyməsini sıxırıq.
- "Run for branch name or tag" bölməsindən lazım olan versiya tag-ını seçirik.
- "Variables" bölməsində, key olaraq `ROLLBACK`, value olaraq isə `true` daxil edirik.
- "Run pipeline" düyməsini sıxırıq.

**Qeyd: Nəzərə alın ki, qeyd olunan açar sözləri istifadə olunmadığı halda ci/cd pipeline-ları işə düşməyəcəkdir.**
