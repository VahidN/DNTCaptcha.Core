// @ts-check
const { defineConfig } = require('eslint/config');
const eslint = require('@eslint/js');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');
const prettier = require('eslint-plugin-prettier/recommended');
const boundariesModule = require('eslint-plugin-boundaries');
// Support the CommonJS export and the package's declared default export.
const boundaries = boundariesModule.default ?? boundariesModule;

module.exports = defineConfig(
  { ignores: ['projects/**', 'dist/**', '.angular/**', 'coverage/**', 'out-tsc/**', 'node_modules/**'] },
  {
    files: ['eslint.config.js'],
    extends: [eslint.configs.recommended, prettier],
    languageOptions: {
      sourceType: 'commonjs',
      globals: { __dirname: 'readonly' }
    }
  },
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
      prettier
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': ['error', { type: 'attribute', prefix: 'app', style: 'camelCase' }],
      '@angular-eslint/component-selector': ['error', { type: 'element', prefix: 'app', style: 'kebab-case' }]
    }
  },
  {
    files: ['src/app/**/*.ts'],
    plugins: { boundaries },
    settings: {
      'boundaries/root-path': __dirname,
      'boundaries/elements': [
        { type: 'core', pattern: 'src/app/core', partialMatch: false },
        { type: 'login', pattern: 'src/app/login', partialMatch: false },
        { type: 'captcha', pattern: 'src/app/dnt-captcha', partialMatch: false }
      ],
      'boundaries/files': [{ pattern: 'src/app/*.ts', category: 'app' }],
      'boundaries/dependency-nodes': ['import', 'export', 'require', 'dynamic-import'],
      'import/resolver': {
        typescript: { project: './tsconfig.app.json' }
      }
    },
    rules: {
      'boundaries/no-unknown-files': 'error',
      'boundaries/no-unknown-dependencies': 'error',
      'boundaries/dependencies': [
        'error',
        {
          default: 'disallow',
          policies: [
            {
              from: { file: { categories: ['app'] } },
              allow: [{ to: { file: { categories: ['app'] } } }, { to: { element: { type: ['login', 'core'] } } }]
            },
            {
              from: { element: { type: ['login', 'captcha'] } },
              allow: { to: { element: { type: 'core' } } }
            },
            {
              from: { element: { type: 'login' } },
              allow: { to: { element: { type: 'captcha' } } }
            }
          ]
        }
      ]
    }
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility, prettier],
    rules: {
      'prettier/prettier': ['error', { parser: 'angular' }]
    }
  }
);
