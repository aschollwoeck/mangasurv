import React from "react";

import Manga from "../components/Manga";
import * as MangaActions from "../actions/MangaActions";
import MangaStore from "../stores/MangaStore";
import SearchBar from "../components/SearchBar"


export default class MangasFollowed extends React.Component {
  constructor() {
    super();
    this.getMangas = this.getMangas.bind(this);
    this.state = {
      mangas: MangaStore.getAllFollowedMangas(),
      filterText: '',
    };

    this.filterMangas = this.filterMangas.bind(this);
  }

  componentWillMount() {
    MangaStore.on("change", this.getMangas);
  }

  componentWillUnmount() {
    MangaStore.removeListener("change", this.getMangas);
  }

  getMangas() {
    this.setState({
      mangas: MangaStore.getAllFollowedMangas(),
    });
  }

  filterMangas(e) {
    this.setState({
      filterText: e
    });   
  }

  render() {
    const { mangas } = this.state;

    return (
      <div>
        <h1>Mangas</h1>
        <SearchBar filterText = {this.state.filterText} onUserInput={this.filterMangas} />

        <div>
          {mangas.map((manga) => {
            return manga.name.toUpperCase().indexOf(this.state.filterText.toUpperCase()) >= 0 ? <Manga key={manga.id} {...manga}/> : "";
          })}
        </div>
      </div>
    );
  }
}