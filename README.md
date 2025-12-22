Application Web de Gestion des Tests 

##  Système de Gestion Industrielle pour Asteel Flash (EMS)

Une application web complète pour la gestion des tests de contrôle qualité dans les environnements de fabrication électronique, mettant en œuvre une validation automatisée du workflow avec une traçabilité complète.

##  Fonctionnalités Principales

###  Workflow Qualité Automatisé à 3 Niveaux
- Validation séquentielle : **Test Initial** → **Contrôle Qualité** → **Test Client**
- Suggestion automatique du prochain test selon l'historique produit
- Rejet après 2 échecs consécutifs au même niveau
- Marquage "Terminé" après réussite du Test Client

###  Interface Multi-Rôles
- **Administrateur** : Gestion des utilisateurs, supervision globale, statistiques,creation Test , rechercher Avancée, export Excel .
- **Utilisateur** : Création de tests, recherche avancée, export Excel
- **Opérateur** : Interface simplifiée, double confirmation (60 secondes)

###  Capacités Techniques
- Vérification en temps réel de l'historique produit
- Attribution automatique des équipements (Board/Machine/Opérateur)
- Recherche avancée avec filtres multiples
- Export Excel avec mise en forme automatique
- Tableaux de bord temps réel avec visualisations Chart.js

## 🛠️ Stack Technologique

### Backend
- **ASP.NET MVC** - Framework web
- **C#** - Logique métier et contrôleurs
- **Entity Framework** - ORM pour la gestion de base de données
- **SQL Server** - Base de données relationnelle

### Frontend
- **HTML5/CSS3** - Structure et style des pages
- **Bootstrap 5** - Framework responsive
- **JavaScript** - Interactivité côté client
- **Chart.js** - Visualisation de données
- **jQuery** - Manipulation du DOM

### Sécurité & Outils
- **BCrypt** - Hachage des mots de passe
- **EPPlus** - Génération de fichiers Excel
- **Visual Studio 2022** - Environnement de développement
- **SQL Server Management Studio** - Gestion de base de données

##  Règles Métier Implémentées

### 1. Workflow Séquentiel
- Progression stricte : Test Initial → Contrôle Qualité → Test Client
- Aucun saut d'étape autorisé
- Le niveau précédent doit réussir pour avancer

### 2. Attribution des Équipements
- **Test Initial** → Board A (Fixture Test Initial)
- **Contrôle Qualité** → Board B (Fixture Contrôle Qualité)  
- **Test Client** → Board C (Fixture Test Client)

### 3. Permissions des Opérateurs
- **Technicien** : Seulement Test Initial & Re-test Initial
- **Ingénieur** : Seulement Contrôle Qualité & Re-contrôle
- **Superviseur** : Tous les types de tests

### 4. Règles de Validation
- Maximum 1 re-test par niveau
- 2 échecs consécutifs = Produit rejeté
- Pas de tests en double du même type
- Le produit doit exister avant test

## 📈 Méthodes Clés

### ProductController.cs
- `CheckTestHistory()` - Vérification produit en temps réel
- `ValiderReglesMetier()` - Validation des règles métier
- `GetSuggestionLogique()` - Suggestion intelligente de test
- `EstProduitRejeteGlobal()` - Détection des rejets
- `EstProduitTermine()` - Vérification de fin de cycle

### Fonctionnalités JavaScript
- Validation de formulaire en temps réel
- Filtrage dynamique des machines/opérateurs
- Compte à rebours pour confirmation opérateur
- Sélection automatique du board selon type de test

# ACCÈS DÉMO

##  ADMIN
- **Nom d'utilisateur:** `admin`
- **Mot de passe:** `$cPpUC3DmM`

##  UTILISATEUR  
- **Nom d'utilisateur:** `ahmed`
- **Mot de passe:** `gSqnFFetpF`

##  OPÉRATEURS

### Technicien (Test Initial)
- **Nom:** Ben Ammar Mohamed
- **Nom d'utilisateur:** `T001`
- **Mot de passe:** `T001`

### Ingénieur (Contrôle Qualité)  
- **Nom:** Dhahri Ali
- **Nom d'utilisateur:** `I001`
- **Mot de passe:** `I001`

### Superviseur (Test Client)
- **Nom:** Masmoudi Houssem
- **Nom d'utilisateur:** `S001`  
- **Mot de passe:** `S001`
  


