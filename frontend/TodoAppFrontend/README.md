# Todo App Frontend

A React + TypeScript frontend for the Todo App, with TypeScript client generated from OpenAPI/Smithy specifications.

## Features

- **Email-based login**: Login with email or create a new account
- **Todo Lists**: Create, view, and delete todo lists
- **Tasks Management**: 
  - Add tasks to lists
  - Mark tasks as complete with checkboxes
  - Edit task descriptions inline (click on the task text)
  - Delete tasks
  - Drag and drop to reorder tasks
- **Persistent Storage**: All data is stored in SQLite database via the backend API

## Getting Started

### Prerequisites

- Node.js 18+ installed
- Backend API running on `http://localhost:5247`

### Installation

```bash
npm install
```

### Development

Start the development server:

```bash
npm run dev
```

The app will be available at `http://localhost:5173`

### Regenerate API Client

If the backend OpenAPI spec changes, regenerate the TypeScript client:

```bash
npm run generate-api
```

## Project Structure

```
src/
├── api/
│   ├── client.ts       # API client wrapper with helper functions
│   └── schema.ts       # Generated TypeScript types from OpenAPI
├── components/
│   ├── Login.tsx       # Login/signup component
│   ├── TodoListView.tsx # Main view with list sidebar
│   └── TaskList.tsx    # Task list with drag-drop support
├── types.ts            # Shared TypeScript interfaces
├── App.tsx             # Root component
└── main.tsx            # Entry point
```

## Usage

1. **Login**: Enter your email to login, or create a new account with email and username
2. **Create Lists**: Use the sidebar to create new todo lists
3. **Add Tasks**: Select a list and add tasks using the input field
4. **Manage Tasks**:
   - Click the checkbox to mark tasks complete
   - Click on task text to edit inline
   - Drag tasks by the handle (⋮⋮) to reorder
   - Click the trash icon to delete
5. **Delete Lists**: Click the × button next to a list name to delete it

## Environment Variables

Create a `.env` file to configure the API URL:

```
VITE_API_URL=http://localhost:5247
```

## Technologies

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **openapi-fetch** - Type-safe API client
- **openapi-typescript** - OpenAPI to TypeScript generator
