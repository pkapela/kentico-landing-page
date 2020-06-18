'use strict';

// Plugins
const gulp = require('gulp');
const rename = require('gulp-rename');
const newer = require('gulp-newer');
const sass = require('gulp-sass');
const cleanCSS = require('gulp-clean-css');
const concat = require('gulp-concat');
const uglify = require('gulp-uglify');
const inject = require('gulp-inject');
const removeDev = require('gulp-remove-html');
var paths = {
  srcFONTS: './src/fonts/**/*',
  buildFONTS: './build/fonts/',
  srcSASS: './src/scss/**/*.scss',
  srcCSS: './src/css/**/*.css',
  destCSS: './src/css/',
  buildCSS: './build/css/',
  srcJS: './src/js/**/*.js',
  buildJS: './build/js/',
  srcHTML: './src/*.*',
  buildHTML: './build/',
  srcIMAGES: './src/img/**/*',
  buildIMAGES: './build/img/'
};

function fontsMove() {
  return gulp
    .src(paths.srcFONTS)
    .pipe(newer(paths.buildFONTS))
    .pipe(gulp.dest(paths.buildFONTS));
  }

function imageMove() {
  return gulp
    .src(paths.srcIMAGES)
    .pipe(newer(paths.buildIMAGES))
    .pipe(gulp.dest(paths.buildIMAGES));
}

function htmlMove() {
  return gulp
    .src(paths.srcHTML)
    .pipe(newer(paths.buildHTML))
    .pipe(gulp.dest(paths.buildHTML));
}

function sassProc() {
  return gulp
    .src(paths.srcSASS)
    .pipe(sass())
    .pipe(gulp.dest(paths.destCSS));
}

function cssProc() {
  return gulp
    .src(paths.srcCSS)
    .pipe(cleanCSS())
    .pipe(rename({suffix: '.min'}))
    .pipe(gulp.dest(paths.buildCSS));
}

function jsProc() {
  return gulp
    .src(['./src/js/core/*.js', './src/js/*.js'])
    .pipe(concat('main.js'))
    .pipe(uglify())
    .pipe(rename({suffix: '.min'}))
    .pipe(gulp.dest(paths.buildJS));
}

function injectProc() {
  var js = gulp.src('./build/js/*.js');
  var css = gulp.src('./build/css/*.css');

  return gulp.src(paths.buildHTML + '*.html')
    .pipe(inject(js, {relative:true} ))
    .pipe(inject(css, {relative:true} ))
    .pipe(removeDev())
    .pipe(gulp.dest(paths.buildHTML));
}

function watchSASS() {
  gulp.watch(paths.srcSASS, sassProc)  
}

exports.watch = watchSASS;
exports.build = gulp.series(htmlMove, fontsMove, imageMove, sassProc, cssProc, jsProc, injectProc);
exports.sass = sassProc;
exports.js = jsProc;
exports.css = cssProc;
