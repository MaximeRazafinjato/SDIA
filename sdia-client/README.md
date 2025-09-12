# SDIA - Système de Gestion des Inscriptions Académiques

Interface utilisateur moderne développée avec React et TypeScript pour le système de gestion des inscriptions académiques.

## 🚀 Technologies Utilisées

- **React 18** - Framework JavaScript pour interface utilisateur
- **TypeScript** - Langage typé pour JavaScript
- **Vite** - Outil de build rapide
- **Material-UI (MUI)** - Bibliothèque de composants UI
- **React Router** - Routing côté client
- **React Query (TanStack Query)** - Gestion des données et cache
- **Axios** - Client HTTP
- **React Hook Form** - Gestion des formulaires
- **Zod** - Validation de schémas
- **Recharts** - Graphiques et visualisation de données
- **date-fns** - Manipulation des dates

## 📁 Structure du Projet

```
src/
├── api/                    # Configuration Axios et API
│   └── client.ts
├── components/             # Composants React
│   ├── common/            # Composants réutilisables
│   └── features/          # Composants métier
├── contexts/              # Contextes React (Auth, etc.)
│   └── AuthContext.tsx
├── hooks/                 # Custom hooks
│   └── useRegistrations.ts
├── layouts/               # Layouts des pages
│   └── MainLayout.tsx
├── pages/                 # Pages de l'application
│   ├── Dashboard.tsx
│   ├── Login.tsx
│   └── Registrations.tsx
├── types/                 # Types TypeScript
│   ├── auth.ts
│   ├── registration.ts
│   └── index.ts
├── utils/                 # Fonctions utilitaires
│   └── formatters.ts
└── App.tsx               # Composant racine
```

## ⚙️ Installation et Configuration

### Prérequis
- Node.js (v16 ou supérieur)
- npm ou yarn

### Installation des dépendances
```bash
npm install
```

### Configuration de l'environnement
Créer un fichier `.env` basé sur `.env.example`:
```bash
cp .env.example .env
```

Configuration par défaut:
- `VITE_API_BASE_URL=https://localhost:7089`

## 🏃‍♂️ Scripts Disponibles

```bash
# Démarrer en mode développement
npm run dev

# Build pour production
npm run build

# Prévisualiser la build de production
npm run preview

# Linter ESLint
npm run lint
```

## 🎨 Fonctionnalités

### 🔐 Authentification
- Page de connexion sécurisée
- Gestion des tokens JWT
- Routes protégées
- Contexte d'authentification global

### 📊 Dashboard
- Statistiques en temps réel
- Graphiques interactifs (barres, secteurs, courbes)
- Cartes de métriques
- Interface responsive

### 📋 Gestion des Inscriptions
- Liste des inscriptions avec DataGrid
- Filtrage par statut
- Vue détaillée des inscriptions
- Actions CRUD complètes

### 🎨 Interface Utilisateur
- Design Material Design moderne
- Thème personnalisable
- Navigation responsive
- Composants accessibles

## 🔧 Configuration Avancée

### Alias de Chemins
Le projet utilise l'alias `@` pour les imports:
```typescript
import { useAuth } from '@/contexts/AuthContext';
import apiClient from '@/api/client';
```

### Configuration TypeScript
- Support complet TypeScript
- Types stricts activés
- Aliases de chemins configurés
- Validation d'imports

## 🌐 API Integration

### Configuration Axios
- Intercepteurs de requêtes/réponses
- Gestion automatique des tokens
- Gestion d'erreurs centralisée
- Timeout configuré

### React Query
- Cache intelligent des données
- Synchronisation automatique
- Gestion d'état des requêtes
- Mutations optimistes

## 📱 Responsiveness

L'application est entièrement responsive et s'adapte à tous les écrans:
- Mobile (< 768px)
- Tablette (768px - 1024px)
- Desktop (> 1024px)

## 🚀 Déploiement

### Build de Production
```bash
npm run build
```

La build génère un dossier `dist/` prêt pour le déploiement.

### Serveur de Développement
```bash
npm run dev
```

L'application sera accessible sur `http://localhost:5173`

## 🔗 Intégration Backend

L'application est configurée pour se connecter à l'API SDIA:
- URL par défaut: `https://localhost:7089`
- Authentification JWT
- Endpoints RESTful

## 📝 Développement

### Conventions de Code
- Composants fonctionnels avec hooks
- TypeScript strict
- Props interfaces définies
- Gestion d'erreurs appropriée

### Structure des Composants
```typescript
interface Props {
  // Props définies
}

const Component: React.FC<Props> = ({ prop1, prop2 }) => {
  // Hooks
  // Logic
  // Return JSX
};

export default Component;
```

## 🤝 Contribution

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/nouvelle-fonctionnalite`)
3. Commit les changements (`git commit -m 'Ajout nouvelle fonctionnalité'`)
4. Push sur la branche (`git push origin feature/nouvelle-fonctionnalite`)
5. Ouvrir une Pull Request

## 📄 License

Ce projet est sous licence MIT.